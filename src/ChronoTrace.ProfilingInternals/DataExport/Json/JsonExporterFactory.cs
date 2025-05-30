using ChronoTrace.ProfilingInternals.Settings;

namespace ChronoTrace.ProfilingInternals.DataExport.Json;

internal static class JsonExporterFactory
{
    internal static JsonExporter MakeJsonExporter(ProfilingSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.OutputPath))
        {
            return new JsonExporter(new StaticExportDirectoryProvider(), new StaticJsonFileNameProvider());
        }

        var directoryProvider = new BuildPropertyExportDirectoryProvider(settings.OutputPath);
        IJsonFileNameProvider fileNameProvider;

        if (Path.EndsInDirectorySeparator(settings.OutputPath) 
            || string.IsNullOrEmpty(Path.GetFileName(settings.OutputPath)))
        {
            fileNameProvider = new StaticJsonFileNameProvider();
        }
        else
        {
            fileNameProvider = new BuildPropertyJsonFileNameProvider(settings.OutputPath);
        }

        return new JsonExporter(directoryProvider, fileNameProvider);
    }
}
