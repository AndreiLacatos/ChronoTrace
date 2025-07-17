using System.Collections.Immutable;
using ChronoTrace.ProfilingInternals.Settings;
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
        var outputPathProvider = context.AnalyzerConfigOptionsProvider.CreateTraceOutputPathProvider();

        var trackedMethodInvocations = GroupInvocationsByClass( 
            GroupInvocationsByMethod(
                SelectTrackedMethodInvocations(
                    SelectMethodInvocations(context.SyntaxProvider),
                    SelectAttributedMethods(context.SyntaxProvider))));

        context.RegisterPostInitializationOutput(GenerateInterceptsLocationAttribute);
        context.RegisterSourceOutput(outputPathProvider, GenerateSettingsProvider);
        context.RegisterSourceOutput(trackedMethodInvocations, GenerateInterceptors);
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

    /// <summary>
    /// Receives the list of methods subject to interception (and their list of invocations). Makes groups (lists) of
    /// methods based on the parent class of the method.
    /// </summary>
    /// <param name="trackedInvocations">List of all method invocations</param>
    /// <returns>Returns a list of lists, where each nested list contains methods that are declared in the same class</returns>
    private static IncrementalValuesProvider<ImmutableArray<InterceptableMethodInvocations>> GroupInvocationsByClass(
        IncrementalValuesProvider<InterceptableMethodInvocations> trackedInvocations)
    {
        return trackedInvocations
            .Collect()
            .SelectMany((invocationsArray, _) =>
            {
                if (invocationsArray.IsEmpty)
                {
                    return Enumerable.Empty<IEnumerable<InterceptableMethodInvocations>>();
                }

                return invocationsArray.GroupBy(
                    invocation => invocation.TargetMethod.OriginalDefinition.ContainingType,
                    SymbolEqualityComparer.Default
                );
            })
            .Select(static (m, _) => m.ToImmutableArray())
            .Where(static m => !m.IsEmpty);
    }

    /// <summary>
    /// Receives the list of invocations of tracked methods. Groups invocations based on the method type
    /// </summary>
    /// <param name="allTrackedInvocations">All identified invocations subject to interception</param>
    /// <returns>List of invocations grouped by method</returns>
    private static IncrementalValuesProvider<InterceptableMethodInvocations> GroupInvocationsByMethod(
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

    /// <summary>
    /// Generates the <c>[InterceptsLocationAttribute]</c>
    /// </summary>
    /// <param name="ctx">Current <c>IncrementalGeneratorPostInitializationContext</c></param>
    private void GenerateInterceptsLocationAttribute(
        IncrementalGeneratorPostInitializationContext ctx)
    {
        ctx.AddSource(
            "ChronoTrace.CompilerUtilities.g.cs",
            new SourceGeneratorUtilities(_dependencies!.TimeProvider).MakeInterceptsLocationAttribute()
        );
    }

    /// <summary>
    /// Generates the profiling interceptors for methods subject to interception. Receives the list of method
    /// invocations grouped by their parent class
    /// </summary>
    /// <param name="context">Current <c>SourceProductionContext</c></param>
    /// <param name="interceptableInvocation">List of method invocations (grouped by their class)</param>
    private void GenerateInterceptors(
        SourceProductionContext context,
        ImmutableArray<InterceptableMethodInvocations> interceptableInvocation)
    {
        var generatedSources = new InterceptorSyntaxGenerator()
            .MakeMethodInterceptors(interceptableInvocation);
        context.AddSource(
            new GeneratedSourceFileNameProvider().GetHintName(interceptableInvocation.First().TargetMethod),
            new SourceGeneratorUtilities(_dependencies!.TimeProvider).FormatCompilationUnitSyntax(generatedSources));
    }

    /// <summary>
    /// Generates the source files for configuring library settings.
    /// </summary>
    /// <param name="context">Current <c>SourceProductionContext</c></param>
    /// <param name="outputPath">Desired setting value for the trace output path</param>
    private void GenerateSettingsProvider(
        SourceProductionContext context,
        string? outputPath)
    {
        var generatedSources = new SettingsProviderSyntaxGenerator()
            .MakeSettingsProvider(outputPath);
        context.AddSource(
            $"{nameof(ProfilingSettingsProvider)}.g.cs",
            new SourceGeneratorUtilities(_dependencies!.TimeProvider).FormatSourceCode(generatedSources));
    }
}
