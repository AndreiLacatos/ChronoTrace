using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;

/// <summary>
/// A utility class responsible for generating a standardized class name
/// for an extension class that will contain profiling interceptor methods related
/// to a specific target type.
/// </summary>
/// <remarks>
/// It ensures a consistent naming convention for generated classes that provide
/// interceptor extensions for methods within a particular user-defined class.
/// </remarks>
internal sealed class InterceptorClassNameProvider
{
    /// <summary>
    /// Generates a class name for an interceptor extension class based on the containing type
    /// of the provided method symbol.
    /// </summary>
    /// <param name="symbol">
    /// The <see cref="IMethodSymbol"/> of a method within the type for which
    /// the interceptor extension class name is being generated.
    /// </param>
    /// <returns>
    /// A string representing the generated class name.
    /// </returns>
    internal string GetClassName(IMethodSymbol symbol) => $"{symbol.ContainingType.Name}ProfilingInterceptorExtensions";
}
