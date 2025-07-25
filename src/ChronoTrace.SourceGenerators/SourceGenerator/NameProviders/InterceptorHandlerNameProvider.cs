using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders
{
    /// <summary>
    /// A utility class responsible for generating a standardized name for an
    /// interceptor method that targets a specific original method from user-code.
    /// </summary>
    /// <remarks>
    /// It ensures a consistent naming convention for the generated methods
    /// that will act as interceptors for user-defined methods.
    /// </remarks>
    internal sealed class InterceptorHandlerNameProvider
    {
        /// <summary>
        /// Generates a name for an interceptor method based on the name of the target method symbol.
        /// </summary>
        /// <param name="symbol">
        /// The <see cref="IMethodSymbol"/> of the original method for which the interceptor
        /// method name is being generated.
        /// </param>
        /// <returns>
        /// A string representing the generated interceptor method name.
        /// </returns>
        internal string GetHandlerName(IMethodSymbol symbol) => $"Intercept{symbol.Name}";
    }
}
