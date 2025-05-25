using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChronoTrace.SourceGenerators;

[Generator]
public class InterceptorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var loggerProvider = context.AnalyzerConfigOptionsProvider.CreateLoggerProvider();

        var attributedMethodsProvider =
            context.SyntaxProvider
                .ForAttributeWithMetadataName(
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
                .Select((symbols, _) => symbols.ToImmutableHashSet(SymbolEqualityComparer.Default))
                .EnrichWithLogger(loggerProvider);

        context.RegisterSourceOutput(attributedMethodsProvider, (_, enrichedAttributedMethods) =>
        {
            var (methodSymbols, logger) = enrichedAttributedMethods;
            logger.Info($"Found {methodSymbols.Count} method(s) decorated with [Profile] attribute");
            logger.Debug("Methods:");
            foreach (var methodSymbol in methodSymbols)
            {
                logger.Debug($"\t{methodSymbol.ContainingType.Name}.{methodSymbol.Name}");
            }
            logger.Flush();
        });
    }
}
