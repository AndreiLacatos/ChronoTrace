using System.Diagnostics;
using ChronoTrace.ProfilingInternals.Compat;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.Compat;

public sealed class StopwatchExtensionTests
{
    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1_000)]
    public async Task GetElapsedTime_AfterSleep_ShouldReturnExpectedElapsedTime(int sleepTimeMs)
    {
        var start = Stopwatch.GetTimestamp();
        await Task.Delay(sleepTimeMs);
        var end = Stopwatch.GetTimestamp();

        var elapsedTime = StopwatchExtensions.GetElapsedTime(start, end);
        
        // elapsed time should be +/-5% or +/-20ms (whichever is greater) of sleep time
        var expected = TimeSpan.FromMilliseconds(sleepTimeMs);
        var tolerance = TimeSpan.FromMilliseconds(Math.Max(sleepTimeMs * 0.05, 20));
        elapsedTime.ShouldBeInRange(expected - tolerance, expected + tolerance);
    }
}
