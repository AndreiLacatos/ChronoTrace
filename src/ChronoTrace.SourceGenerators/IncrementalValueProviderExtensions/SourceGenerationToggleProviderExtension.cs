using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static System.Boolean;

namespace ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions
{
    /// <summary>
    /// Defines the source generation toggle type
    /// </summary>
    internal interface ISourceGenerationToggle
    {
    }

    /// <summary>
    /// Type corresponding to enabled source generation
    /// </summary>
    internal sealed class SourceGenerationEnabled : ISourceGenerationToggle
    {
    }

    /// <summary>
    /// Type corresponding to disabled source generation
    /// </summary>
    internal sealed class SourceGenerationDisabled : ISourceGenerationToggle
    {
    }

    /// <summary>
    /// Provides extension methods for creating an <see cref="IncrementalValueProvider{T}"/>
    /// that supplies the toggle enabling/disabling source generation.
    /// </summary>
    internal static class SourceGenerationToggleProviderExtension
    {
        /// <summary>
        /// Creates an <see cref="IncrementalValueProvider{ISourceGenerationToggle}"/> that provides source generation toggle.
        /// </summary>
        /// <param name="valueProvider">
        /// An <see cref="IncrementalValueProvider{AnalyzerConfigOptionsProvider}"/> which gives access
        /// to global MSBuild properties.
        /// </param>
        /// <returns>
        /// An <see cref="IncrementalValueProvider{String}"/> that yields source generation toggle.
        /// </returns>
        internal static IncrementalValueProvider<ISourceGenerationToggle> CreateSourceGenerationToggleProvider(
            this IncrementalValueProvider<AnalyzerConfigOptionsProvider> valueProvider)
        {
            return valueProvider.Select((provider, _) =>
            {
                var sourceGenerationToggleDefined = provider.GlobalOptions.TryGetValue(
                    Constants.BuildProperties.SourceGenerationToggle,
                    out var toggleValue);
                if (sourceGenerationToggleDefined && !string.IsNullOrWhiteSpace(toggleValue))
                {
                    return TryParse(toggleValue, out var isEnabled) && isEnabled
                        ? new SourceGenerationEnabled()
                        : new SourceGenerationDisabled() as ISourceGenerationToggle;
                }

                // enable source generation when neither is defined 
                return new SourceGenerationEnabled(); 
            });
        }
    }
}
