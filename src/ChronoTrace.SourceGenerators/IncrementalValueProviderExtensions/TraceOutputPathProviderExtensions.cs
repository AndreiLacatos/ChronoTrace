using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions
{
    /// <summary>
    /// Provides extension methods for creating an <see cref="IncrementalValueProvider{T}"/>
    /// that supplies the configured output path for trace reports.
    /// </summary>
    /// <remarks>
    /// This class is used within the source generation pipeline to access MSBuild properties
    /// that specify where the <c>ChronoTrace</c> library should save its output files.
    /// </remarks>
    internal static class TraceOutputPathProviderExtensions
    {
        /// <summary>
        /// Creates an <see cref="IncrementalValueProvider{String}"/> that provides the configured
        /// output path for trace/timing reports, read from global analyzer config options.
        /// </summary>
        /// <param name="valueProvider">
        /// An <see cref="IncrementalValueProvider{AnalyzerConfigOptionsProvider}"/> which gives access
        /// to global MSBuild properties.
        /// </param>
        /// <returns>
        /// An <see cref="IncrementalValueProvider{String}"/> that yields the value of the
        /// corresponding MSBuild property.
        /// Returns <c>null</c> if the property is not defined or not found.
        /// </returns>
        internal static IncrementalValueProvider<string?> CreateTraceOutputPathProvider(
            this IncrementalValueProvider<AnalyzerConfigOptionsProvider> valueProvider)
        {
            return valueProvider.Select((provider, _) =>
            {
                provider.GlobalOptions.TryGetValue(Constants.BuildProperties.TimingOutputPath, out var outputPath);
                return outputPath;
            });
        }
    }
}
