namespace ChronoTrace.SourceGenerators.Tests;

public class SettingsInitializerTests
{
    [Fact]
    public async Task SettingsProvided_ShouldBeForwardedDuringInitialization()
    {
        var analyzerOptionsProvider = new MockAnalyzerConfigOptionsProvider(new Dictionary<string, string>
        {
            { "build_property.ChronoTraceTimingOutput", @"dir\sub-dir\timing-report-file.json" },
        });
        var (driver, _) = SourceGenerationRunner.Run(string.Empty, analyzerOptionsProvider);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task SettingsProvided_StdoutKeywordAsOutputPath_ShouldCreateStdoutSettings()
    {
        var analyzerOptionsProvider = new MockAnalyzerConfigOptionsProvider(new Dictionary<string, string>
        {
            { "build_property.ChronoTraceTimingOutput", "stdout" },
        });
        var (driver, _) = SourceGenerationRunner.Run(string.Empty, analyzerOptionsProvider);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
