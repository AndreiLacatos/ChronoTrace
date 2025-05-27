using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;

internal sealed class InterceptorHandlerNameProvider
{
    internal string GetHandlerName(IMethodSymbol symbol) => $"Intercept{symbol.Name}";
}