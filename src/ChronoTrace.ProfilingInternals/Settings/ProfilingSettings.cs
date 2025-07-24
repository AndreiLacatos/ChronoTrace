using ChronoTrace.ProfilingInternals.Protection;
using ChronoTrace.ProfilingInternals.Settings.DataExport;

namespace ChronoTrace.ProfilingInternals.Settings;

/// <summary>
/// Holds static configuration data for the <c>ChronoTrace</c> profiler.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[LibraryUsage]
public sealed class ProfilingSettings
{
    /// <summary>
    /// Data export configuration.
    /// </summary>
    public required IDataExportSettings DataExportSettings { get; init; }

    /// <summary>
    /// Static factory method which provides the default settings.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [LibraryUsage]
    public static ProfilingSettings Default => new ProfilingSettings
    {
        DataExportSettings = new JsonExporterSettings
        {
            OutputPath = null,
        },
    };
}
