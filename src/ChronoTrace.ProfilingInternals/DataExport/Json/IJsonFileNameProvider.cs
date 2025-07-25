namespace ChronoTrace.ProfilingInternals.DataExport.Json
{
    /// <summary>
    /// Defines a contract for components that provide the specific
    /// file name to be used for JSON output files (e.g. trace reports).
    /// </summary>
    internal interface IJsonFileNameProvider
    {
        /// <summary>
        /// Retrieves the desired file name for a JSON export.
        /// </summary>
        /// <remarks>
        /// This includes the extension but not the directory path!
        /// </remarks>
        /// <returns>
        /// A string representing the file name (e.g. "report.json", "trace_data_2023-10-27.json").
        /// </returns>
        string GetJsonFileName();
    }
}
