namespace ChronoTrace.ProfilingInternals.Settings.DataExport
{
    /// <summary>
    /// Defines settings for exporting trace data to JSON.
    /// </summary>
    public sealed class JsonExporterSettings : IDataExportSettings
    {
        /// <summary>
        /// Marks the location where traces are output.
        /// </summary>
        public string? OutputPath { get; set; }
    }
}
