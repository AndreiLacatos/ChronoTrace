using ChronoTrace.ProfilingInternals.DataExport.FileRotation;
using ChronoTrace.ProfilingInternals.Settings;

namespace ChronoTrace.ProfilingInternals.DataExport.Json;

/// <summary>
/// An internal factory responsible for creating and configuring instances of <see cref="JsonExporter"/>.
/// It uses <see cref="ProfilingSettings"/> to determine the appropriate directory and file name providers.
/// </summary>
internal static class JsonExporterFactory
{
    /// <summary>
    /// Creates and configures a <see cref="JsonExporter"/> based on the provided profiling settings.
    /// </summary>
    /// <param name="settings">The <see cref="ProfilingSettings"/> containing configuration,
    /// particularly the <see cref="ProfilingSettings.OutputPath"/>.</param>
    /// <returns>A fully configured <see cref="JsonExporter"/> instance.</returns>
    internal static JsonExporter MakeJsonExporter(ProfilingSettings settings)
    {
        var fileRotation = new DailyCounterFileRotationStrategy(TimeProvider.System);
        if (string.IsNullOrWhiteSpace(settings.OutputPath))
        {
            return new JsonExporter(
                new StaticExportDirectoryProvider(),
                new StaticJsonFileNameProvider(),
                fileRotation);
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

        return new JsonExporter(directoryProvider, fileNameProvider, fileRotation);
    }
}
