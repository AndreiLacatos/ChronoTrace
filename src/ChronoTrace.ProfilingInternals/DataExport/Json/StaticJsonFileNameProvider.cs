namespace ChronoTrace.ProfilingInternals.DataExport.Json;

internal sealed class StaticJsonFileNameProvider : IJsonFileNameProvider
{
    public string GetJsonFileName() => "report.json";
}