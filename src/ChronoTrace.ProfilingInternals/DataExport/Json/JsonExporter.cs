using System.Text.Json;

namespace ChronoTrace.ProfilingInternals.DataExport.Json;

internal sealed class JsonExporter : ITraceVisitor
{
    private TimingReport? _timingReport;
    private readonly IExportDirectoryProvider _exportDirectoryProvider;
    private readonly IJsonFileNameProvider _jsonFileNameProvider;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    internal JsonExporter(
        IExportDirectoryProvider exportDirectoryProvider,
        IJsonFileNameProvider jsonFileNameProvider)
    {
        _exportDirectoryProvider = exportDirectoryProvider;
        _jsonFileNameProvider = jsonFileNameProvider;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
    }

    public void BeginVisit()
    {
        _timingReport = new TimingReport
        {
            MethodTimings = new List<TimingReport.MethodTiming>(),
        };
    }

    public void VisitTrace(Trace trace)
    {
        var timing = new TimingReport.MethodTiming
        {
            MethodName = trace.MethodName,
            ExecutionTime = trace.ExecutionTime,
        };
        _timingReport!.MethodTimings.Add(timing);
    }

    public void Complete()
    {
        var json = JsonSerializer.Serialize(_timingReport, _jsonSerializerOptions);
        var directory = _exportDirectoryProvider.GetExportDirectory();
        var path = Path.Combine(
            directory,
            _jsonFileNameProvider.GetJsonFileName());
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(path, json);
    }
}
