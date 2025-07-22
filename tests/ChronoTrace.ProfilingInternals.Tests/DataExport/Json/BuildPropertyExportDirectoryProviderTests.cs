using ChronoTrace.ProfilingInternals.DataExport.Json;
using ChronoTrace.ProfilingInternals.Tests.XunitExtensions.PlatformAwareDataAttributes;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public class BuildPropertyExportDirectoryProviderTests
{
    [Theory]
    [WindowsData(@"C:\temp\traces\output\")]
    [LinuxData("/var/log/traces/output/")]
    public void GetExportDirectory_WithFullPath_ShouldReturnFullPath(string fullPath)
    {
        // Arrange
        var provider = new BuildPropertyExportDirectoryProvider(fullPath);

        // Act
        var result = provider.GetExportDirectory();

        // Assert
        result.ShouldBe(fullPath);
    }

    [Theory]
    [WindowsData(@"logs\traces\")]
    [LinuxData("/logs/traces/")]
    public void GetExportDirectory_WithRelativePath_ShouldReturnRelativePath(string relativePath)
    {
        // Arrange
        var provider = new BuildPropertyExportDirectoryProvider(relativePath);

        // Act
        var result = provider.GetExportDirectory();

        // Assert
        result.ShouldBe(relativePath);
    }

    [Theory]
    [WindowsData(@"traces\")]
    [LinuxData("traces/")]
    public void GetExportDirectory_WithDirectoryNameOnly_ShouldReturnSameDirectory(string directoryName)
    {
        // Arrange
        var provider = new BuildPropertyExportDirectoryProvider(directoryName);

        // Act
        var result = provider.GetExportDirectory();

        // Assert
        result.ShouldBe(directoryName);
    }

    [Fact]
    public void GetExportDirectory_WithEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var emptyPath = string.Empty;
        var provider = new BuildPropertyExportDirectoryProvider(emptyPath);

        // Act
        var result = provider.GetExportDirectory();

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Theory]
    [WindowsData(@"C:\logs\traces\")]
    [LinuxData("/var/logs/traces/")]
    public void GetExportDirectory_CalledMultipleTimes_ShouldReturnConsistentResult(string path)
    {
        // Arrange
        var provider = new BuildPropertyExportDirectoryProvider(path);

        // Act
        var result1 = provider.GetExportDirectory();
        var result2 = provider.GetExportDirectory();
        var result3 = provider.GetExportDirectory();

        // Assert
        result1.ShouldBe(path);
        result2.ShouldBe(path);
        result3.ShouldBe(path);
        result1.ShouldBe(result2);
        result2.ShouldBe(result3);
    }

    [Theory]
    [WindowsData(@"C:\Program Files\MyApp\logs\")]
    [WindowsData(@"C:\logs with spaces\traces\")]
    [WindowsData(@"logs-with-dashes\")]
    [WindowsData(@"logs.with.dots\")]
    [LinuxData("logs-with-dashes/")]
    [LinuxData("logs.with.dots/")]
    [LinuxData("logs_with_underscores/")]
    public void GetExportDirectory_WithVariousDirectoryNames_ShouldReturnExactPath(string directoryPath)
    {
        // Arrange
        var provider = new BuildPropertyExportDirectoryProvider(directoryPath);

        // Act
        var result = provider.GetExportDirectory();

        // Assert
        result.ShouldBe(directoryPath);
    }
}
