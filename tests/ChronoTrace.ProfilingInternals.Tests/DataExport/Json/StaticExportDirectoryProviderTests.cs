using ChronoTrace.ProfilingInternals.DataExport.Json;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public class StaticExportDirectoryProviderTests
{
    [Fact]
    public void GetExportDirectory_ShouldReturnTimings()
    {
        // Arrange
        var provider = new StaticExportDirectoryProvider();

        // Act
        var result = provider.GetExportDirectory();

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        
        // Ensure the result is a valid directory name (no invalid path characters)
        var invalidChars = Path.GetInvalidPathChars();
        result.ShouldNotContain(c => invalidChars.Contains(c));
    }
}