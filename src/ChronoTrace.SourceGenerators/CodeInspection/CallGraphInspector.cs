using System.Collections.Immutable;
using ChronoTrace.SourceGenerators.DataStructures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChronoTrace.SourceGenerators.CodeInspection;

/// <summary>
/// Utilities for inspecting the call graph of a set of methods.
/// </summary>
internal static class CallGraphInspector
{
    /// <summary>
    /// Traverses the call graph of a given set of methods to find all reachable methods.
    /// </summary>
    /// <param name="initialMethods">The starting set of methods to analyze.</param>
    /// <param name="compilation">The compilation object, required for semantic analysis.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An immutable hash set containing all methods that are called directly or
    /// indirectly by the initial set of methods, including the initial methods themselves.</returns>
    internal static ImmutableArray<MethodInvocation> DiscoverAmbientMethods(
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
    internal static List<MethodInvocation> GatherMethodCalls(
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

                var caller = invocation
                    .Ancestors()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault();

                // capture the invocation location of the method call
                var metadata = new MethodMetadata(originalDefinition.GetMethodType(semanticModel.Compilation));
#pragma warning disable RSEXPERIMENTAL002
                if (semanticModel.GetInterceptableLocation(invocation, cancellationToken) is { } location)
                {
                    calledMethods.Add(new MethodInvocation(
                        originalDefinition,
                        invocation.GetLocation(),
                        caller?.Identifier.Text,
                        location,
                        metadata));
                }
#pragma warning restore RSEXPERIMENTAL002

                // recursively discover all method calls within the called method
                calledMethods.AddRange(GatherMethodCalls(originalDefinition, compilation, seenMethods, cancellationToken));
            }
        }

        return calledMethods;
    }
}