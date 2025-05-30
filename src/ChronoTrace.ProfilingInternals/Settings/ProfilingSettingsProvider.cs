namespace ChronoTrace.ProfilingInternals.Settings;

public static class ProfilingSettingsProvider
{
    private static ProfilingSettings _settings = ProfilingSettings.Default;
    internal static ProfilingSettings GetSettings() => _settings;

    public static void UpdateSettings(ProfilingSettings settings)
    {
        _settings = settings;
    }
}
