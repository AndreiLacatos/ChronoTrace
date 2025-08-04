using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChronoTrace.SourceGenerators.CodeInspection;

/// <summary>
/// Provides filter for attributed methods.
/// </summary>
internal static class AttributedMethodFilter
{
    /// <summary>
    /// Filters all methods that are attributed with the library specific <c>[ProfileAttribute]</c>.
    /// </summary>
    /// <param name="syntaxProvider">Roslyn syntax provider</param>
    /// <returns>The list of attributed methods</returns>
    internal static IncrementalValueProvider<ImmutableHashSet<ISymbol>> FilterAttributedMethods(
        this SyntaxValueProvider syntaxProvider)
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
}
