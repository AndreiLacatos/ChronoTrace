using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;

public class InterceptorClassNameProvider
{
    internal string GetClassName(IMethodSymbol symbol) => $"{symbol.ContainingType.Name}ProfilingInterceptorExtensions";
}