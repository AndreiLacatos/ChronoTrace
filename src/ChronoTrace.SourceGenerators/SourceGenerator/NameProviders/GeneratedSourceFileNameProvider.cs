using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;

/// <summary>
/// A provider responsible for generating unique and descriptive "hint names"
/// for source files created by a Roslyn source generator. These hint names are used
/// when adding generated source code to the compilation.
/// </summary>
internal sealed class GeneratedSourceFileNameProvider
{
    /// <summary>
    /// Generates a unique hint name for a source file based on the provided method symbol.
    /// The name is intended to be used with <c>SourceProductionContext.AddSource()</c>.
    /// </summary>
    /// <param name="symbol">The <see cref="IMethodSymbol"/> for the method for which
    /// a source file (e.g. an interceptor) is being generated.</param>
    /// <returns>
    /// A string representing the generated hint name.
    /// </returns>
    internal string GetHintName(IMethodSymbol symbol)
    {
        var className = symbol.ContainingType.Name;
        var methodName = symbol.Name;
        return $"{className}_{methodName}_ProfilingInterceptors.g.cs";
    }
}
