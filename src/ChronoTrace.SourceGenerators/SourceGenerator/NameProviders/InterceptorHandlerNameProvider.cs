using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;

/// <summary>
/// A stateful utility class responsible for generating a unique, standardized name
/// for an interceptor method that targets a specific original method from user-code.
/// </summary>
/// <remarks>
/// <para>
/// This class ensures a consistent and unique naming convention for generated interceptors.
/// It must be instantiated once and reused throughout a single source generation pass
/// to correctly track method encounters and generate unique suffixes.
/// </para>
/// </remarks>
internal sealed class InterceptorHandlerNameProvider
{
    private readonly Dictionary<IMethodSymbol, int> _seenSymbols;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterceptorHandlerNameProvider"/>.
    /// </summary>
    public InterceptorHandlerNameProvider()
    {
        // Use SymbolEqualityComparer to correctly handle different instances
        // of IMethodSymbol that refer to the same method definition.
        _seenSymbols = new Dictionary<IMethodSymbol, int>(SymbolEqualityComparer.Default);
    }

    /// <summary>
    /// Generates a unique name for an interceptor method based on the target method symbol.
    /// If the same method symbol is provided multiple times, a unique, zero-padded suffix
    /// will be appended (e.g., "_000001").
    /// </summary>
    /// <param name="symbol">
    /// The <see cref="IMethodSymbol"/> of the original method for which the interceptor
    /// method name is being generated.
    /// </param>
    /// <returns>
    /// A string representing the unique generated interceptor method name.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the same method is encountered more than 999,999 times, exceeding the suffix limit.
    /// </exception>
    internal string GetHandlerName(IMethodSymbol symbol)
    {
        var originalDefinition = symbol.OriginalDefinition;
        var currentCount = _seenSymbols.GetValueOrDefault(originalDefinition, 1);

        _seenSymbols[originalDefinition] = currentCount + 1;
        
        return $"Intercept{originalDefinition.Name}_{currentCount:D6}";
    }
}
