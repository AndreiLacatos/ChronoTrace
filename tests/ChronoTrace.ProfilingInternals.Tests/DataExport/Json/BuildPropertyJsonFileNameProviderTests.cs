using ChronoTrace.ProfilingInternals.DataExport.Json;
using ChronoTrace.ProfilingInternals.Tests.XunitExtensions.PlatformAwareDataAttributes;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public class BuildPropertyJsonFileNameProviderTests
{
    [Theory]
    [WindowsData(@"C:\temp\traces\output.json")]
    [LinuxData("/var/log/traces/output.json")]
    public void GetJsonFileName_WithFullPath_ShouldReturnOnlyFileName(string fullPath)
    {
        // Arrange
        var provider = new BuildPropertyJsonFileNameProvider(fullPath);

        // Act
        var result = provider.GetJsonFileName();

        // Assert
        result.ShouldBe("output.json");
    }

    [Theory]
    [WindowsData(@"logs\traces.json")]
    [LinuxData("logs/traces.json")]
    public void GetJsonFileName_WithRelativePath_ShouldReturnFileName(string relativePath)
    {
        // Arrange
        var provider = new BuildPropertyJsonFileNameProvider(relativePath);

        // Act
        var result = provider.GetJsonFileName();

        // Assert
        result.ShouldBe("traces.json");
    }

    [Fact]
    public void GetJsonFileName_WithFileNameOnly_ShouldReturnSameFileName()
    {
        // Arrange
        var fileName = "traces.json";
        var provider = new BuildPropertyJsonFileNameProvider(fileName);

        // Act
        var result = provider.GetJsonFileName();

        // Assert
        result.ShouldBe("traces.json");
    }

    [Fact]
    public void GetJsonFileName_WithEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var emptyPath = string.Empty;
        var provider = new BuildPropertyJsonFileNameProvider(emptyPath);

        // Act
        var result = provider.GetJsonFileName();

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Theory]
    [WindowsData(@"C:\logs\traces.json")]
    [LinuxData("/logs/traces.json")]
    public void GetJsonFileName_CalledMultipleTimes_ShouldReturnConsistentResult(string path)
    {
        // Arrange
        var provider = new BuildPropertyJsonFileNameProvider(path);

        // Act
        var result1 = provider.GetJsonFileName();
        var result2 = provider.GetJsonFileName();
        var result3 = provider.GetJsonFileName();

        // Assert
        result1.ShouldBe("traces.json");
        result2.ShouldBe("traces.json");
        result3.ShouldBe("traces.json");
        result1.ShouldBe(result2);
        result2.ShouldBe(result3);
    }
}