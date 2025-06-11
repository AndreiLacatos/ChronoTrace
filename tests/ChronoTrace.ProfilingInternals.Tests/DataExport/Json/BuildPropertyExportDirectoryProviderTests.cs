using ChronoTrace.ProfilingInternals.DataExport.Json;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public class BuildPropertyExportDirectoryProviderTests
{
    [Theory]
    [InlineData(@"C:\temp\traces\output\")]
    [InlineData("/var/log/traces/output/")]
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
    [InlineData(@"logs\traces\")]
    public void GetExportDirectory_WithRelativePath_ShouldReturnRelativePath(string relativePath)
    {
        // Arrange
        var provider = new BuildPropertyExportDirectoryProvider(relativePath);

        // Act
        var result = provider.GetExportDirectory();

        // Assert
        result.ShouldBe(relativePath);
    }

    [Fact]
    public void GetExportDirectory_WithDirectoryNameOnly_ShouldReturnSameDirectory()
    {
        // Arrange
        var directoryName = @"traces\";
        var provider = new BuildPropertyExportDirectoryProvider(directoryName);

        // Act
        var result = provider.GetExportDirectory();

        // Assert
        result.ShouldBe(@"traces\");
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

    [Fact]
    public void GetExportDirectory_CalledMultipleTimes_ShouldReturnConsistentResult()
    {
        // Arrange
        var path = @"C:\logs\traces\";
        var provider = new BuildPropertyExportDirectoryProvider(path);

        // Act
        var result1 = provider.GetExportDirectory();
        var result2 = provider.GetExportDirectory();
        var result3 = provider.GetExportDirectory();

        // Assert
        result1.ShouldBe(@"C:\logs\traces\");
        result2.ShouldBe(@"C:\logs\traces\");
        result3.ShouldBe(@"C:\logs\traces\");
        result1.ShouldBe(result2);
        result2.ShouldBe(result3);
    }

    [Theory]
    [InlineData(@"C:\Program Files\MyApp\logs\")]
    [InlineData(@"C:\logs with spaces\traces\")]
    [InlineData(@"logs-with-dashes\")]
    [InlineData("logs-with-dashes/")]
    [InlineData("logs.with.dots/")]
    [InlineData(@"logs.with.dots\")]
    [InlineData("logs_with_underscores/")]
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
