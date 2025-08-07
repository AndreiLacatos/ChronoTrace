namespace ChronoTrace.SourceGenerators;

/// <summary>
/// Internal global constants
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Name of the package.
    /// </summary>
    internal const string ChronoTrace = "ChronoTrace";

    /// <summary>
    /// Name of the configuration option that enables recursive tracing.
    /// </summary>
    internal const string RecursiveConfig = "Recursive";

    /// <summary>
    /// String prefix for fully qualified type names.
    /// </summary>
    internal const string GlobalNamespacePrefix = "global::";

    /// <summary>
    /// Fully qualified name of the <c>[Profile]</c> attribute.
    /// </summary>
    internal const string ProfileAttribute = "ChronoTrace.Attributes.ProfileAttribute";

    internal static class BuildProperties
    {
        /// <summary>
        /// Roslyn analyzer config option name for solution directory.
        /// </summary>
        internal const string SolutionDir = "build_property.SolutionDir";

        /// <summary>
        /// Roslyn analyzer config option name for trace log output path.
        /// </summary>
        internal const string TimingOutputPath = "build_property.ChronoTraceTimingOutput";

        /// <summary>
        /// Roslyn analyzer config option name for the library version.
        /// </summary>
        internal const string PackageVersion = "build_property.PackageVersion";

        /// <summary>
        /// Roslyn analyzer config option name for the library version.
        /// </summary>
        internal const string SourceGenerationToggle = "build_property.ChronoTraceSourceGenerationEnabled";
    }
}
