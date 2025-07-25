namespace ChronoTrace.ProfilingInternals.DataExport
{
    /// <summary>
    /// Defines a contract for components responsible for determining
    /// the directory path where exported data (e.g. trace reports) should be saved.
    /// </summary>
    internal interface IExportDirectoryProvider
    {
        /// <summary>
        /// Retrieves the designated directory path for exports.
        /// </summary>
        /// <returns>
        /// A string representing the absolute or relative path to the export directory.
        /// </returns>
        string GetExportDirectory();
    }
}
