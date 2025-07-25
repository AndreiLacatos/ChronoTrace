using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions
{
    /// <summary>
    /// Provides extension methods for creating an <see cref="IncrementalValueProvider{T}"/>
    /// that supplies the version of the <c>ChronoTrace</c> library.
    /// </summary>
    internal static class VersionProviderExtension
    {
        /// <summary>
        /// Creates an <see cref="IncrementalValueProvider{String}"/> that provides the package version.
        /// </summary>
        /// <param name="valueProvider">
        /// An <see cref="IncrementalValueProvider{AnalyzerConfigOptionsProvider}"/> which gives access
        /// to global MSBuild properties.
        /// </param>
        /// <returns>
        /// An <see cref="IncrementalValueProvider{String}"/> that yields the package version.
        /// </returns>
        internal static IncrementalValueProvider<string> CreateVersionProvider(
            this IncrementalValueProvider<AnalyzerConfigOptionsProvider> valueProvider)
        {
            return valueProvider.Select((provider, _) =>
            {
                provider.GlobalOptions.TryGetValue(Constants.BuildProperties.PackageVersion, out var version);
                return string.IsNullOrWhiteSpace(version) ? "Unknown version" : version;
            });
        }
    }
}
