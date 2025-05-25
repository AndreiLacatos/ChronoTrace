using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;

internal class InterceptorHandlerNameProvider
{
    internal string GetHandlerName(IMethodSymbol symbol) => $"Intercept{symbol.Name}";
}