using System.Text.Json;
using ChronoTrace.ProfilingInternals.DataExport.FileRotation;

namespace ChronoTrace.ProfilingInternals.DataExport.Json;

/// <summary>
/// An internal implementation of <see cref="ITraceVisitor"/> that collects trace data
/// and exports it as a JSON file. It utilizes providers for determining the output
/// directory and file name, and a strategy for file rotation.
/// </summary>
internal sealed class JsonExporter : ITraceVisitor
{
    private List<Trace>? _traces;
    private readonly IExportDirectoryProvider _exportDirectoryProvider;
    private readonly IJsonFileNameProvider _jsonFileNameProvider;
    private readonly IFileRotationStrategy _fileRotator;
    private readonly CallGraphBuilder _callGraphBuilder;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private static readonly SemaphoreSlim FileSystemLock = new SemaphoreSlim(1, 1);

    internal JsonExporter(
        IExportDirectoryProvider exportDirectoryProvider,
        IJsonFileNameProvider jsonFileNameProvider,
        IFileRotationStrategy fileRotator,
        CallGraphBuilder callGraphBuilder)
    {
        _exportDirectoryProvider = exportDirectoryProvider;
        _jsonFileNameProvider = jsonFileNameProvider;
        _fileRotator = fileRotator;
        _callGraphBuilder = callGraphBuilder;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
    }

    public void BeginVisit()
    {
        _traces = new List<Trace>(capacity: 100);
    }

    public void VisitTrace(Trace trace)
    {
        _traces?.Add(trace);
    }

    public void Complete()
    {
        var callGraph = _callGraphBuilder.AssembleCallGraph(_traces ?? []);
        var json = JsonSerializer.Serialize(TimingReportMapper.Map(callGraph), _jsonSerializerOptions);
        var directory = _exportDirectoryProvider.GetExportDirectory();
        FileSystemLock.Wait();
        try
        {
            var fileName = _fileRotator.RotateName(
                directory,
                _jsonFileNameProvider.GetJsonFileName());
            var path = Path.Combine(
                directory,
                fileName);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(path, json);
        }
        finally
        {
            FileSystemLock.Release();
        }
    }
}
