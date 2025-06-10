using NSubstitute;
using System.Collections.Concurrent;
using ChronoTrace.ProfilingInternals.DataExport;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests;

public partial class ProfilingContextTests
{
    /// <summary>
    /// Verifies that multiple threads can safely profile methods at the same
    /// time without corrupting data or causing race conditions.
    /// </summary>
    [Fact]
    public async Task ConcurrentMethodProfiling_ShouldHandleMultipleThreads()
    {
        // Arrange
        const int concurrentTasks = 100;
        var tasks = new List<Task>();
        var invocationIds = new ConcurrentBag<ushort>();

        // Act
        // Start multiple concurrent method profiling
        for (var i = 0; i < concurrentTasks; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var id = _profilingContext.BeginMethodProfiling("TestMethod");
                invocationIds.Add(id);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                _profilingContext.EndMethodProfiling(id);
                _profilingContext.CollectTraces();
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        invocationIds.Count.ShouldBe(concurrentTasks);
        invocationIds.ShouldNotContain((ushort)0);
        invocationIds.ShouldBeUnique();
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(concurrentTasks).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
    }

    /// <summary>
    /// Ensures that the trace collection is intelligently delayed when methods
    /// are still running to prevent incomplete data from being processed.
    /// </summary>
    [Fact]
    public async Task ConcurrentCollectTraces_WhileMethodsArePending_ShouldNotProcess()
    {
        // Arrange
        const int methodCount = 10;
        var completionSource = new TaskCompletionSource();
        var methodsStarted = new TaskCompletionSource();

        // Act
        // Start a task that will hold some methods in the "running" state
        var longRunningTask = Task.Run(async () =>
        {
            var ids = new List<ushort>();
            for (var i = 0; i < methodCount; i++)
            {
                ids.Add(_profilingContext.BeginMethodProfiling("LongRunningMethod"));
            }
            
            methodsStarted.SetResult();
            await completionSource.Task;
            
            foreach (var id in ids)
            {
                _profilingContext.EndMethodProfiling(id);
            }
        });

        // Wait for methods to start
        await methodsStarted.Task;

        // Try to collect traces while methods are still running
        var collectTask = Task.Run(() => _profilingContext.CollectTraces());
        await collectTask;

        // Assert - no traces should have been collected yet
        _mockVisitor.DidNotReceive().BeginVisit();
        _mockVisitor.DidNotReceive().VisitTrace(Arg.Any<Trace>());
        _mockVisitor.DidNotReceive().Complete();

        // Cleanup
        completionSource.SetResult();
        await longRunningTask;
        
        // Now collect traces
        _profilingContext.CollectTraces();

        // Assert - traces should be collected after methods complete
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(methodCount).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
    }
}
