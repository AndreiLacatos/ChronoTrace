using ChronoTrace.ProfilingInternals.DataExport.Json;
using ChronoTrace.ProfilingInternals.Settings;

namespace ChronoTrace.ProfilingInternals;

public sealed class ProfilingContextAccessor
{
    private static readonly AsyncLocal<ProfilingContext?> Context = new AsyncLocal<ProfilingContext?>();

    public static ProfilingContext Current
    {
        get
        {
            var visitor = JsonExporterFactory.MakeJsonExporter(ProfilingSettingsProvider.GetSettings());
            Context.Value ??= new ProfilingContext(visitor);
            return Context.Value;
        }
    }
}
