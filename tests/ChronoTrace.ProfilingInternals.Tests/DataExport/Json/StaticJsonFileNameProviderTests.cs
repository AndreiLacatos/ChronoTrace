using ChronoTrace.ProfilingInternals.DataExport.Json;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public class StaticJsonFileNameProviderTests
{
    [Fact]
    public void GetJsonFileName_ShouldReturnReportJson()
    {
        // Arrange
        var provider = new StaticJsonFileNameProvider();

        // Act
        var result = provider.GetJsonFileName();

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        
        // Ensure the result is a valid directory name (no invalid path characters)
        var invalidChars = Path.GetInvalidFileNameChars();
        result.ShouldNotContain(c => invalidChars.Contains(c));
    }
}