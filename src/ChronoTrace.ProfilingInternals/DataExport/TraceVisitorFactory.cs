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
            switch (settings.DataExportSettings)
            {
                case JsonExporterSettings jsonExporterSettings:
                    return JsonExporterFactory.MakeJsonExporter(jsonExporterSettings);
                case StdoutExportSettings _:
                    return new StdoutExporter();
                default:
                    return new DiscardVisitor();
            }
        }
    }
}
