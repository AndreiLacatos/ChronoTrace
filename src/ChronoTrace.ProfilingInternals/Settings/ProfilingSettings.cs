namespace ChronoTrace.ProfilingInternals.Settings;

public sealed class ProfilingSettings
{
    public required string? OutputPath { get; init; }

    public static ProfilingSettings Default => new ProfilingSettings
    {
        OutputPath = null,
    };
}
