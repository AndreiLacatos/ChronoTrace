using System.Collections.Immutable;
using ChronoTrace.ProfilingInternals.Settings;
using ChronoTrace.SourceGenerators.DataStructures;
using ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions;
using ChronoTrace.SourceGenerators.SourceGenerator;
using ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static ChronoTrace.SourceGenerators.SourceGenerationGuard;

namespace ChronoTrace.SourceGenerators;

[Generator]
public class InterceptorGenerator : IIncrementalGenerator
{
    private GeneratorDependencies? _dependencies;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        ConfigureDependencies(GeneratorDependencies.Default);
        var outputPathProvider = context.AnalyzerConfigOptionsProvider.CreateTraceOutputPathProvider();
        var versionProvider = context.AnalyzerConfigOptionsProvider.CreateVersionProvider();
        var sourceGenerationToggleProvider = context.AnalyzerConfigOptionsProvider.CreateSourceGenerationToggleProvider();

        var traced = SelectTrackedMethodInvocations(
            SelectMethodInvocations(context.SyntaxProvider),
            SelectAttributedMethods(context.SyntaxProvider)
                .Combine(context.CompilationProvider)
                .Select((data, cancellationToken) =>
                {
                    var (methods, compilation) = data;

                    if (methods.IsEmpty)
                    {
                        return ImmutableHashSet<ISymbol>.Empty;
                    }

                    // traverse the call graph
                    return CollectAllCalledMethods(
                        methods,
                        compilation,
                        cancellationToken);
                }));

        var trackedMethodInvocations = GroupInvocationsByClass(GroupInvocationsByMethod(traced))
            .Combine(versionProvider);

        context.RegisterSourceOutput(
            versionProvider.WithSourceGenerationToggle(sourceGenerationToggleProvider),
            WhenSourceGenerationEnabled<string>(GenerateInterceptsLocationAttribute));
        context.RegisterSourceOutput(
            outputPathProvider.Combine(versionProvider).WithSourceGenerationToggle(sourceGenerationToggleProvider),
            WhenSourceGenerationEnabled<(string? Left, string Right)>(GenerateSettingsProvider));
        context.RegisterSourceOutput(
            trackedMethodInvocations.WithSourceGenerationToggle(sourceGenerationToggleProvider),
            WhenSourceGenerationEnabled<(ImmutableArray<InterceptableMethodInvocations> Left, string Right)>(GenerateInterceptors));
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
    /// <param name="version">Library version</param>
    private void GenerateInterceptsLocationAttribute(
        SourceProductionContext ctx,
        string version)
    {
        ctx.AddSource(
            "ChronoTrace.CompilerUtilities.g.cs",
            new SourceGeneratorUtilities(_dependencies!.TimeProvider, version).MakeInterceptsLocationAttribute()
        );
    }

    /// <summary>
    /// Generates the profiling interceptors for methods subject to interception. Receives the list of method
    /// invocations grouped by their parent class
    /// </summary>
    /// <param name="context">Current <c>SourceProductionContext</c></param>
    /// <param name="props">Tuple of the list of method invocations (grouped by their class) and the library version</param>
    private void GenerateInterceptors(
        SourceProductionContext context,
        (ImmutableArray<InterceptableMethodInvocations> Left, string Right) props)
    {
        var (interceptableInvocation, version) = props;
        var generatedSources = new InterceptorSyntaxGenerator()
            .MakeMethodInterceptors(interceptableInvocation);
        context.AddSource(
            new GeneratedSourceFileNameProvider().GetHintName(interceptableInvocation.First().TargetMethod),
            new SourceGeneratorUtilities(_dependencies!.TimeProvider, version).FormatCompilationUnitSyntax(generatedSources));
    }

    /// <summary>
    /// Generates the source files for configuring library settings.
    /// </summary>
    /// <param name="context">Current <c>SourceProductionContext</c></param>
    /// <param name="props">Tuple consisting of desired setting value for the trace output path and the library version</param>
    private void GenerateSettingsProvider(
        SourceProductionContext context,
        (string? Left, string Right) props)
    {
        var (outputPath, version) = props;
        var settings = new SettingsProviderSyntaxGenerator.ExportSettings(
            outputPath?.Trim().Equals("stdout", StringComparison.InvariantCultureIgnoreCase) ?? false 
                ? SettingsProviderSyntaxGenerator.TraceOutput.Stdout
                : SettingsProviderSyntaxGenerator.TraceOutput.Json,
            outputPath);
        var generatedSources = new SettingsProviderSyntaxGenerator(version)
            .MakeSettingsProvider(settings);
        context.AddSource(
            $"{nameof(ProfilingSettingsProvider)}.g.cs",
            new SourceGeneratorUtilities(_dependencies!.TimeProvider, version).FormatSourceCode(generatedSources));
    }

    /// <summary>
    /// Traverses the call graph of a given set of methods to find all reachable methods.
    /// </summary>
    /// <param name="initialMethods">The starting set of methods to analyze.</param>
    /// <param name="compilation">The compilation object, required for semantic analysis.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An immutable hash set containing all methods that are called directly or
    /// indirectly by the initial set of methods, including the initial methods themselves.</returns>
    private static ImmutableHashSet<ISymbol> CollectAllCalledMethods(
        ImmutableHashSet<ISymbol> initialMethods,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        var userCodeAssembly = compilation.Assembly;

        // holds methods whose bodies still need to be scanned for further method calls
        var methodsToProcess = new Queue<IMethodSymbol>();

        // tracks every unique method that was found (avoids duplicate
        // processing and prevents infinite loops from recursive method calls)
        var seenMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

        // start with the attributed methods
        foreach (var symbol in initialMethods)
        {
            // sanity check
            if (symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            // work with the original definition of the method to correctly
            // handle generics and treat all instantiations as the same method
            var originalDefinition = methodSymbol.OriginalDefinition;

            // if the method has not been encountered yet, enqueue it for processing
            if (seenMethods.Add(originalDefinition))
            {
                methodsToProcess.Enqueue(originalDefinition);
            }
        }

        // dequeue items and process them
        while (methodsToProcess.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentMethod = methodsToProcess.Dequeue();

            // find where the method is declared in the source code
            foreach (var syntaxRef in currentMethod.DeclaringSyntaxReferences)
            {
                var methodSyntaxNode = syntaxRef.GetSyntax(cancellationToken);

                // the semantic model is needed to process its syntax tree
                var semanticModel = compilation.GetSemanticModel(methodSyntaxNode.SyntaxTree);

                // find every method call within the body of the current method and enqueue them for later processing
                var methodCalls = methodSyntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>();
                foreach (var invocation in methodCalls)
                {
                    // use the semantic model to determine exactly which method is being called
                    var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);

                    // sanity check
                    if (symbolInfo.Symbol is not IMethodSymbol calledMethodSymbol)
                    {
                        continue;
                    }

                    var originalDefinition = calledMethodSymbol.OriginalDefinition;
                    var isUserCode = SymbolEqualityComparer.Default.Equals(
                        originalDefinition.ContainingAssembly,
                        userCodeAssembly);

                    // enqueue for future processing if the method has not been seen yet
                    if (isUserCode && seenMethods.Add(originalDefinition))
                    {
                        methodsToProcess.Enqueue(originalDefinition);
                    }
                }
            }
        }

        // return the complete set of discovered methods
        return seenMethods.ToImmutableHashSet(SymbolEqualityComparer.Default)!;
    }
}
