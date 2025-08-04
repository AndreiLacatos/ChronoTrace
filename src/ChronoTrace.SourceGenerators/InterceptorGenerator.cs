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

        var attributedMethods = SelectAttributedMethods(context.SyntaxProvider);
        var adjacentMethodInvocations = attributedMethods
            .Combine(context.CompilationProvider)
            .Select((data, cancellationToken) => DiscoverAmbientMethods(data.Left, data.Right, cancellationToken));

        var attributedMethodInvocations = SelectTrackedMethodInvocations(
            SelectMethodInvocations(context.SyntaxProvider),
            attributedMethods);

        var trackedMethodInvocations = GroupInvocationsByClass( 
                GroupInvocationsByMethod(attributedMethodInvocations.Collect()
                    .Combine(adjacentMethodInvocations).Select((data, _) => data.Left.AddRange(data.Right))
                    .SelectMany((x, _) => x)))
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
    private static ImmutableArray<MethodInvocation> DiscoverAmbientMethods(
        ImmutableHashSet<ISymbol> initialMethods,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        // iterate over the list of attributed methods, decide which should be profiled recursively
        var methodsToRecursivelyProfile = new List<IMethodSymbol>();
        foreach (var attributedMethod in initialMethods)
        {
            // sanity check
            if (attributedMethod is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            var profileAttribute = methodSymbol
                .GetAttributes()
                .FirstOrDefault(attribute => attribute.AttributeClass?
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Split(Constants.GlobalNamespacePrefix)[1] == Constants.ProfileAttribute);
            
            var recursiveConfig = profileAttribute?.NamedArguments
                .FirstOrDefault(arg => arg.Key == Constants.RecursiveConfig).Value;

            if (recursiveConfig?.Value is true)
            {
                // bingo, method needs recursive profiling
                methodsToRecursivelyProfile.Add(methodSymbol);
            }
        }

        // process each attributed method, gather the list of method calls
        var ambientMethods = new List<MethodInvocation>();
        foreach (var method in methodsToRecursivelyProfile)
        {
            ambientMethods.AddRange(GatherMethodCalls(method, compilation, [], cancellationToken));
        }

        return [..ambientMethods.ToHashSet()];
    }

    /// <summary>
    /// Given a method, recursively discovers all method calls within the method body.
    /// </summary>
    /// <param name="method">Target method</param>
    /// <param name="compilation">Roslyn compilation unit</param>
    /// <param name="seenMethods">List of methods encountered during recursive call tree traversal</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flat list of <see cref="MethodInvocation"/> objects for all method calls in the call tree
    /// starting with the given method</returns>
    private static List<MethodInvocation> GatherMethodCalls(
        IMethodSymbol method,
        Compilation compilation,
        List<IMethodSymbol> seenMethods,
        CancellationToken cancellationToken)
    {
        if (seenMethods.Contains(method, SymbolEqualityComparer.Default))
        {
            return [];
        }

        // mark the method as seen
        seenMethods.Add(method);

        var calledMethods = new List<MethodInvocation>();

        foreach (var syntaxRef in method.OriginalDefinition.DeclaringSyntaxReferences)
        {
            // find every method call within the body of the current method and enqueue them for later processing
            var methodSyntaxNode = syntaxRef.GetSyntax(cancellationToken);
            var methodCalls = methodSyntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in methodCalls)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return [];
                }

                // the semantic model is needed to process its syntax tree
                var semanticModel = compilation.GetSemanticModel(methodSyntaxNode.SyntaxTree);
                // use the semantic model to determine exactly which method is being called
                var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);

                // sanity check
                if (symbolInfo.Symbol is not IMethodSymbol calledMethodSymbol)
                {
                    continue;
                }

                var originalDefinition = calledMethodSymbol.OriginalDefinition;

                var userCodeAssembly = compilation.Assembly;
                var isUserCode = SymbolEqualityComparer.Default.Equals(
                    originalDefinition.ContainingAssembly,
                    userCodeAssembly);
                if (!isUserCode)
                {
                    // ensure only user code is processed
                    continue;
                }

                // capture the invocation location of the method call
                var metadata = new MethodMetadata(originalDefinition.GetMethodType(semanticModel.Compilation));
#pragma warning disable RSEXPERIMENTAL002 // / Experimental interceptable location API
                if (semanticModel.GetInterceptableLocation(invocation, cancellationToken) is { } location)
                {
                    calledMethods.Add(new MethodInvocation(originalDefinition, invocation.GetLocation(), location, metadata));
                }
#pragma warning restore RSEXPERIMENTAL002

                // recursively discover all method calls within the called method
                calledMethods.AddRange(GatherMethodCalls(originalDefinition, compilation, seenMethods, cancellationToken));
            }
        }

        return calledMethods;
    }
}
