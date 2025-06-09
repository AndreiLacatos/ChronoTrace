using System.Collections.Immutable;
using ChronoTrace.SourceGenerators.DataStructures;
using ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions;
using ChronoTrace.SourceGenerators.SourceGenerator;
using ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChronoTrace.SourceGenerators;

[Generator]
public class InterceptorGenerator : IIncrementalGenerator
{
    private GeneratorDependencies? _dependencies;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        ConfigureDependencies(GeneratorDependencies.Default);
        var loggerProvider = context.AnalyzerConfigOptionsProvider.CreateLoggerProvider();
        var outputPathProvider = context.AnalyzerConfigOptionsProvider.CreateTraceOutputPathProvider();

        var trackedMethodInvocations = GroupInvocations(
                SelectTrackedMethodInvocations(
                    SelectMethodInvocations(context.SyntaxProvider),
                    SelectAttributedMethods(context.SyntaxProvider)))
            .EnrichWithLogger(loggerProvider);

        context.RegisterPostInitializationOutput(ctx => 
            GenerateInterceptsLocationAttribute(ctx, _dependencies!.TimeProvider));
        context.RegisterSourceOutput(outputPathProvider, (ctx, payload) => 
            GenerateSettingsProvider(ctx, payload, _dependencies!.TimeProvider));
        context.RegisterSourceOutput(trackedMethodInvocations, (ctx, payload) => 
            GenerateInterceptors(ctx, payload, _dependencies!.TimeProvider));
    }

    internal void ConfigureDependencies(GeneratorDependencies dependencies)
    {
        _dependencies ??= dependencies;
    }

    private static IncrementalValueProvider<ImmutableHashSet<ISymbol>> SelectAttributedMethods(SyntaxValueProvider syntaxProvider)
    {
        return syntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: Constants.ProfileAttribute,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) =>
                {
                    if (ctx.TargetSymbol is IMethodSymbol methodSymbol)
                    {
                        return methodSymbol;
                    }

                    return null;
                })
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .Select(ISymbol (m, _) => m.OriginalDefinition)
            .Collect()
            .Select((symbols, _) => symbols.ToImmutableHashSet(SymbolEqualityComparer.Default));
    }

    private static IncrementalValuesProvider<MethodInvocation> SelectMethodInvocations(SyntaxValueProvider syntaxProvider)
    {
        return syntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is InvocationExpressionSyntax,
                transform: static (ctx, ct) =>
                {
                    var invocationSyntax = (InvocationExpressionSyntax)ctx.Node;
                    var symbolInfo = ctx.SemanticModel.GetSymbolInfo(invocationSyntax, ct);

                    IMethodSymbol? targetMethodSymbol = null;
                    if (symbolInfo.Symbol is IMethodSymbol directSymbol)
                    {
                        targetMethodSymbol = directSymbol.OriginalDefinition;
                    }

                    if (targetMethodSymbol == null)
                    {
                        return null;
                    }

                    var metadata = new MethodMetadata(targetMethodSymbol.GetMethodType(ctx.SemanticModel.Compilation));

#pragma warning disable RSEXPERIMENTAL002 // / Experimental interceptable location API
                    if (ctx.SemanticModel.GetInterceptableLocation(invocationSyntax, ct) is { } location)
                    {
                        return new MethodInvocation(targetMethodSymbol, invocationSyntax.GetLocation(), location, metadata);
                    }
#pragma warning restore RSEXPERIMENTAL002

                    return null;
                })
            .Where(static x => x is not null)
            .Select((x, _) => x!);
    }

    private static IncrementalValuesProvider<MethodInvocation> SelectTrackedMethodInvocations(
        IncrementalValuesProvider<MethodInvocation> allInvocations,
        IncrementalValueProvider<ImmutableHashSet<ISymbol>> attributedMethods)
    {
        return attributedMethods
            .Combine(allInvocations.Collect())
            .SelectMany((tuple, _) =>
            {
                var (attributedSet, invocationSet) = tuple;
                var relevantInvocations = new List<MethodInvocation>();
                foreach (var invInfo in invocationSet)
                {
                    if (attributedSet.Contains(invInfo.TargetMethod))
                    {
                        relevantInvocations.Add(invInfo);
                    }
                }
                return relevantInvocations;
            });
    }

    private static IncrementalValuesProvider<InterceptableMethodInvocations> GroupInvocations(
        IncrementalValuesProvider<MethodInvocation> allTrackedInvocations)
    {
        return allTrackedInvocations
            .Collect()
            .SelectMany((invocationsArray, _) => invocationsArray
                .GroupBy(inv => inv.TargetMethod, SymbolEqualityComparer.Default)
                .Select(group => new InterceptableMethodInvocations(
                    (IMethodSymbol)group.Key!,
                    group.Select(item => (item.Location, item.InterceptableLocation)),
                    group.First().Metadata))
            );
    }

    private static void GenerateInterceptsLocationAttribute(
        IncrementalGeneratorPostInitializationContext ctx,
        TimeProvider timeProvider)
    {
        ctx.AddSource(
            "ChronoTrace.CompilerUtilities.g.cs",
            new SourceGeneratorUtilities(timeProvider).MakeInterceptsLocationAttribute()
        );
    }
    
    private static void GenerateInterceptors(
        SourceProductionContext context,
        (InterceptableMethodInvocations, Logger) enrichedInvocation,
        TimeProvider timeProvider)
    {
        var (interceptableInvocation, logger) = enrichedInvocation;
        var generatedSources = new InterceptorSyntaxGenerator(logger)
            .MakeMethodInterceptor(interceptableInvocation);
        context.AddSource(
            new GeneratedSourceFileNameProvider().GetHintName(interceptableInvocation.TargetMethod),
            new SourceGeneratorUtilities(timeProvider).FormatCompilationUnitSyntax(generatedSources));
    }

    private static void GenerateSettingsProvider(
        SourceProductionContext context,
        string? outputPath,
        TimeProvider timeProvider)
    {
        var generatedSources = new SettingsProviderSyntaxGenerator()
            .MakeSettingsProvider(outputPath);
        context.AddSource(
            "ProfilingSettingsProvider.g.cs",
            new SourceGeneratorUtilities(timeProvider).FormatSourceCode(generatedSources));
    }
}
