using Shouldly;

namespace ChronoTrace.SourceGenerators.Tests;

public class SourceGenerationToggleTests
{
    [Theory]
    [InlineData("false")]
    [InlineData("0")]
    [InlineData("FALSE")]
    [InlineData("False")]
    [InlineData("fAlsE")]
    public void WhenSourceGenerationDisabledViaBuildPropery_NoOutputShouldBeGenerated(string configValue)
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public void Do()
                {
                }
            }

            var subject = new S();
            subject.Do();
            """;
        
        var analyzerOptionsProvider = new MockAnalyzerConfigOptionsProvider(new Dictionary<string, string>
        {
            { "build_property.ChronoTraceSourceGenerationEnabled", configValue },
        });

        var (driver, _) = SourceGenerationRunner.Run(source, analyzerOptionsProvider);
        var generatedTrees = driver.GetRunResult().GeneratedTrees;

        generatedTrees.ShouldBeEmpty();
    }
}