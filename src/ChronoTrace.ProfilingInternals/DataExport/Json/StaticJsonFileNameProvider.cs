namespace ChronoTrace.ProfilingInternals.DataExport.Json
{
    /// <summary>
    /// An internal implementation of <see cref="IJsonFileNameProvider"/> that
    /// always returns a predefined JSON file name.
    /// This is typically used as a default when no specific output file name is derived from configuration.
    /// </summary>
    internal sealed class StaticJsonFileNameProvider : IJsonFileNameProvider
    {
        public string GetJsonFileName() => "report.json";
    }
}
