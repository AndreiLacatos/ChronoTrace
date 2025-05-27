using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;

internal sealed class InterceptorClassNameProvider
{
    internal string GetClassName(IMethodSymbol symbol) => $"{symbol.ContainingType.Name}ProfilingInterceptorExtensions";
}