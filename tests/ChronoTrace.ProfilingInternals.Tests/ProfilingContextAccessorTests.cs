namespace ChronoTrace.ProfilingInternals.Tests;

public class ProfilingContextAccessorTests
{
    [Fact]
    public void ProfilingContextAccessor_Current_ShouldYieldNonNull()
    {
        var context = ProfilingContextAccessor.Current;
        Assert.NotNull(context);
    }

    [Fact]
    public void ProfilingContextAccessor_SyncContextSuccessiveCalls_ShouldYieldSameInstance()
    {
        var one = ProfilingContextAccessor.Current;
        var two = ProfilingContextAccessor.Current;
        Assert.Same(one, two);
    }

    [Fact]
    public async Task ProfilingContextAccessor_AsyncContextSuccessiveCalls_ShouldYieldSameInstance()
    {
        var one = ProfilingContextAccessor.Current;
        await Task.Delay(TimeSpan.FromMilliseconds(20));
        var two = ProfilingContextAccessor.Current;
        Assert.Same(one, two);
    }

    [Fact]
    public async Task ProfilingContextAccessor_DifferentContextSuccessiveCalls_ShouldYieldSameInstance()
    {
        var taskOne = Task.Run(() => ProfilingContextAccessor.Current);
        var taskTwo = Task.Run(() => ProfilingContextAccessor.Current);
        var results = await Task.WhenAll(taskOne, taskTwo);
        var one = results[0];
        var two = results[1];
        Assert.NotSame(one, two);
    }
}
