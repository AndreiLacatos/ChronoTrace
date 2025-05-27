namespace ChronoTrace.ProfilingInternals.DataExport.Json;

internal sealed class StaticExportDirectoryProvider : IExportDirectoryProvider
{
    public string GetExportDirectory() => "timings";
}
