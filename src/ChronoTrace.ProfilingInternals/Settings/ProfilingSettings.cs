using ChronoTrace.ProfilingInternals.Protection;

namespace ChronoTrace.ProfilingInternals.Settings;

/// <summary>
/// Holds static configuration data for the <c>ChronoTrace</c> profiler.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[LibraryUsage]
public sealed class ProfilingSettings
{
    /// <summary>
    /// Marks the location where traces are output.
    /// </summary>
    public required string? OutputPath { get; init; }

    /// <summary>
    /// Static factory method which provides the default settings.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [LibraryUsage]
    public static ProfilingSettings Default => new ProfilingSettings
    {
        OutputPath = null,
    };
}
