using ChronoTrace.SourceGenerators.DataStructures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChronoTrace.SourceGenerators.CodeInspection;

/// <summary>
/// Provides filters for method invocations.
/// </summary>
internal static class InvocationFilter
{
    /// <summary>
    /// Filters all method invocations, combines syntax and semantic information.
    /// </summary>
    /// <param name="syntaxProvider">Roslyn syntax provider</param>
    /// <returns>List of <see cref="MethodInvocation"/> objects, each item corresponds to an actual method invocation.</returns>
    internal static IncrementalValuesProvider<MethodInvocation> FilterMethodInvocations(
        this SyntaxValueProvider syntaxProvider)
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

                    var caller = invocationSyntax
                        .Ancestors()
                        .OfType<MethodDeclarationSyntax>()
                        .FirstOrDefault();
                    
#pragma warning disable RSEXPERIMENTAL002
                    if (ctx.SemanticModel.GetInterceptableLocation(invocationSyntax, ct) is { } location)
                    {
                        return new MethodInvocation(
                            targetMethodSymbol,
                            invocationSyntax.GetLocation(),
                            caller?.Identifier.Text,
                            location,
                            metadata);
                    }
#pragma warning restore RSEXPERIMENTAL002

                    return null;
                })
            .Where(static x => x is not null)
            .Select((x, _) => x!);
    }
}
