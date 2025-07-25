using ChronoTrace.ProfilingInternals.DataExport.Json;
using ChronoTrace.ProfilingInternals.DataExport.Stdout;
using ChronoTrace.ProfilingInternals.Settings;
using ChronoTrace.ProfilingInternals.Settings.DataExport;

namespace ChronoTrace.ProfilingInternals.DataExport
{
    internal static class TraceVisitorFactory
    {
        internal static ITraceVisitor MakeTraceVisitor(ProfilingSettings settings)
        {
            return settings.DataExportSettings switch
            {
                JsonExporterSettings jsonExporterSettings => JsonExporterFactory.MakeJsonExporter(jsonExporterSettings),
                StdoutExportSettings _ => new StdoutExporter(),
                _ => new DiscardVisitor(),
            };
        }
    }
}
