using ChronoTrace.ProfilingInternals.DataExport.FileRotation;

namespace ChronoTrace.ProfilingInternals.DataExport.Json;

/// <summary>
/// An internal implementation of <see cref="IExportDirectoryProvider"/> that
/// always returns a predefined directory name.
/// This is typically used as a default when no specific output directory is configured.
/// </summary>
internal sealed class StaticExportDirectoryProvider : IExportDirectoryProvider
{
    public string GetExportDirectory() => "timings";
}
