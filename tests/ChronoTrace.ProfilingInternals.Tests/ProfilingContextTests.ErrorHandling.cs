using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests;

public partial class ProfilingContextTests
{
    /// <summary>
    /// Confirms that the profiling context gracefully handles invalid
    /// method IDs without throwing exceptions or crashing.
    /// </summary>
    [Fact]
    public void EndMethodProfiling_WithInvalidId_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        var exception = Record.Exception(() =>
        {
            _profilingContext.EndMethodProfiling(0);
            _profilingContext.EndMethodProfiling(ushort.MaxValue);
        });

        exception.ShouldBeNull();
    }
}
