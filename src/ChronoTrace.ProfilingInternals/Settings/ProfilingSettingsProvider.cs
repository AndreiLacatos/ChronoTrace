using ChronoTrace.ProfilingInternals.Protection;

namespace ChronoTrace.ProfilingInternals.Settings
{
    /// <summary>
    /// Provides access to static settings of the <c>ChronoTrace</c> profiler.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [LibraryUsage]
    public static class ProfilingSettingsProvider
    {
        /* 
         * This class provides various build time settings; bridges the compile time world
         * with the runtime. The source generator generates a [ModuleInitializer] which
         * packs the build time settings and passes them to UpdateSettings(). It ensures
         * that build properties are passed correctly to the runtime; therefore, it had to
         * be exposed publicly, even though it is not necessarily part of the intended
         * exposed public API (yet).
         *
         * Consumer code should not interact with this by design.
         * 
         */

        /// <summary>
        /// Private instance of the settings.
        /// </summary>
        private static ProfilingSettings _settings = ProfilingSettings.Default;
        
        /// <summary>
        /// Access the current settings.
        /// </summary>
        /// <returns>Current settings</returns>
        internal static ProfilingSettings GetSettings() => _settings;


        /// <summary>
        /// Modifies internal settings for the ChronoTrace library.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   WARNING: INTERNAL USE ONLY.
        /// </para>
        /// <para>
        ///   This method is intended exclusively for internal management by the <c>ChronoTrace</c> library.
        ///   Consumer applications should NOT call this method directly.
        /// </para>
        /// <para>
        ///   Invoking this method externally, especially multiple times during runtime, can lead to
        ///   unpredictable behavior, incorrect trace data, or instability.
        ///   The library manages its own configuration data.
        /// </para>
        /// </remarks>
        /// <param name="settings">Desired settings</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [LibraryUsage]
        public static void UpdateSettings(ProfilingSettings settings)
        {
            _settings = settings;
        }
    }
}
