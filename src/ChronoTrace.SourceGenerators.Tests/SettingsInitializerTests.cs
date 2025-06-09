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
        var driver = SourceGenerationRunner.Run(string.Empty, analyzerOptionsProvider);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
