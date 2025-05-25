using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;

internal sealed class GeneratedSourceFileNameProvider
{
    internal string GetHintName(IMethodSymbol symbol)
    {
        var className = symbol.ContainingType.Name;
        var methodName = symbol.Name;
        return $"{className}_{methodName}_ProfilingInterceptors.g.cs";
    }
}
