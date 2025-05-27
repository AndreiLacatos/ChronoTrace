using ChronoTrace.ProfilingInternals.DataExport.Json;

namespace ChronoTrace.ProfilingInternals;

public sealed class ProfilingContextAccessor
{
    private static readonly AsyncLocal<ProfilingContext?> Context = new AsyncLocal<ProfilingContext?>();

    public static ProfilingContext Current
    {
        get
        {
            var exporter = new JsonExporter(new StaticExportDirectoryProvider(), new StaticJsonFileNameProvider());
            Context.Value ??= new ProfilingContext(exporter);
            return Context.Value;
        }
    }
}
