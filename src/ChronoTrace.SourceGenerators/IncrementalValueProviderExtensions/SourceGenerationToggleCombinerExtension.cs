using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions
{
    /// <summary>
    /// Simple object enhancing an actual value with a source generation toggle instance
    /// </summary>
    /// <param name="SourceGenerationToggle">Source generation toggle instance, indicates
    /// whether source generation is enabled or not</param>
    /// <param name="Value">Actual value</param>
    /// <typeparam name="TValue">Type of the actual value</typeparam>
    internal sealed class SourceGenerationToggleWrappedValue<TValue>
    {
        internal ISourceGenerationToggle SourceGenerationToggle { get; set; } = null!;
        internal TValue Value { get; set; } = default!;
    }

    /// <summary>
    /// Provides QOL improving extension methods for creating an <see cref="IncrementalValueProvider{T}"/>
    /// enhanced with source generation toggle.
    /// </summary>
    internal static class SourceGenerationToggleCombinerExtension
    {
        /// <summary>
        /// Enhances an <see cref="IncrementalValueProvider{T}"/> with the source generation toggle
        /// </summary>
        /// <param name="provider">Provider of the actual value</param>
        /// <param name="sourceGenerationToggleProvider">Provider of source generation toggle</param>
        /// <typeparam name="TValue">Enhanced <see cref="IncrementalValueProvider{T}"/></typeparam>
        /// <returns></returns>
        internal static IncrementalValueProvider<SourceGenerationToggleWrappedValue<TValue>> WithSourceGenerationToggle<TValue>(
            this IncrementalValueProvider<TValue> provider,
            IncrementalValueProvider<ISourceGenerationToggle> sourceGenerationToggleProvider)
        {
            return provider
                .Combine(sourceGenerationToggleProvider)
                .Select((data, _) => 
                    new SourceGenerationToggleWrappedValue<TValue>
                    {
                        SourceGenerationToggle = data.Right,
                        Value = data.Left,
                    });
        }

        /// <summary>
        /// Enhances an <see cref="IncrementalValuesProvider{T}"/> with the source generation toggle
        /// </summary>
        /// <param name="provider">Provider of the actual values</param>
        /// <param name="sourceGenerationToggleProvider">Provider of source generation toggle</param>
        /// <typeparam name="TValue">Enhanced <see cref="IncrementalValuesProvider{T}"/></typeparam>
        /// <returns></returns>
        internal static IncrementalValuesProvider<SourceGenerationToggleWrappedValue<TValue>> WithSourceGenerationToggle<TValue>(
            this IncrementalValuesProvider<TValue> provider, // Input is a COLLECTION provider
            IncrementalValueProvider<ISourceGenerationToggle> sourceGenerationToggleProvider)
        {
            return provider
                .Combine(sourceGenerationToggleProvider) 
                .Select((data, _) =>
                    new SourceGenerationToggleWrappedValue<TValue>
                    {
                        SourceGenerationToggle = data.Right,
                        Value = data.Left,
                    });
        }
    }
}
