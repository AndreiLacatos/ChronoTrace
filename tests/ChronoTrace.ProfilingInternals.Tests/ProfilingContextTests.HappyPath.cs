using ChronoTrace.ProfilingInternals.DataExport;
using NSubstitute;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests;

public partial class ProfilingContextTests
{
    /// <summary>
    /// Tests the simplest happy path use case:
    ///     <c>BeginMethodProfiling</c>
    ///     <c>EndMethodProfiling</c>
    ///     <c>CollectTraces</c>
    /// are called in succession with valid parameters in the right order.
    /// </summary>
    [Fact]
    public void ContextActionsInvoked_ShouldYieldTraces()
    {
        const string methodName = "SomeTestMethod";
        var id = _profilingContext.BeginMethodProfiling(methodName);
        ((int)id).ShouldNotBe(0);
        _profilingContext.EndMethodProfiling(id);
        _profilingContext.CollectTraces();

        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(1).Complete();
        _mockVisitor.Received(1).VisitTrace(Arg.Any<Trace>());
        Received.InOrder(() =>
        {
            _mockVisitor.BeginVisit();
            _mockVisitor.VisitTrace(Arg.Any<Trace>());
            _mockVisitor.Complete();
        });
    }

    /// <summary>
    /// Tests the scenario of nested method calls that are profiled.
    ///     MethodAlpha()
    ///         MethodBravo()
    ///             MethodCharlie()
    ///                 ...
    ///             end MethodCharlie
    ///         end MethodBravo
    ///     end MethodAlpha
    /// Context actions are called with valid parameters in the right order.
    /// </summary>
    [Theory]
    [InlineData("MethodAlpha", "MethodBravo")]
    [InlineData("MethodAlpha", "MethodBravo", "MethodCharlie")]
    [InlineData("MethodAlpha", "MethodBravo", "MethodCharlie", "MethodDelta", "MethodEcho", "MethodFox", "MethodGolf")]
    public void SimulateNestedCalls_ContextActionsInvoked_ShouldYieldTraces(params string[] methodNames)
    {
        var names = methodNames.ToList();
        var ids = names.Select(_profilingContext.BeginMethodProfiling).ToList();
        ids.Count.ShouldBe(names.Count);
        ids.ShouldNotContain((ushort)0);
        ids.ShouldBeUnique();
        ids.Reverse();
        foreach (var id in ids)
        {
            _profilingContext.EndMethodProfiling(id);
            _profilingContext.CollectTraces();
        }

        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(1).Complete();
        _mockVisitor.Received(names.Count).VisitTrace(Arg.Any<Trace>());
        Received.InOrder(() =>
        {
            _mockVisitor.BeginVisit();
            foreach (var name in names)
            {
                _mockVisitor.VisitTrace(Arg.Is<Trace>(trace => trace.MethodName == name));
            }
            _mockVisitor.Complete();
        });
    }

    /// <summary>
    /// Proves that an out-of-order method completion is handled gracefully.
    /// </summary>
    [Fact]
    public void CallStackLogic_WithOutOfOrderCompletion_AssignsIncorrectCallers()
    {
        // Arrange
        var capturedTraces = new List<Trace>();
        _mockVisitor.When(v => v.VisitTrace(Arg.Any<Trace>()))
                  .Do(callInfo => capturedTraces.Add(callInfo.Arg<Trace>()));

        // Act
        // This sequence is designed to corrupt the stack and then immediately
        // use the corrupted state to produce a measurable error.

        // 1. A standard nested call: Root -> Alpha
        var rootId = _profilingContext.BeginMethodProfiling("Root");
        var alphaId = _profilingContext.BeginMethodProfiling("Alpha");

        // 2. An out-of-order completion: The parent (Root) finishes before its child (Alpha).
        // This is an unusual but valid scenario in complex async code.
        _profilingContext.EndMethodProfiling(rootId);
        
        // 3. A new method is profiled. At this point, the conceptual "top" of the stack
        // should be nothing (since Root has returned), but the flawed queue's "top" is "Alpha".
        // When "Bravo" begins, it will Peek() the corrupted stack and incorrectly
        // identify "Alpha" as its caller.
        var bravoId = _profilingContext.BeginMethodProfiling("Bravo");

        // 4. Cleanup: The remaining methods finish.
        _profilingContext.EndMethodProfiling(alphaId);
        _profilingContext.EndMethodProfiling(bravoId);
        _profilingContext.CollectTraces();

        // Assert
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(3).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();

        var alphaTrace = capturedTraces.Single(t => t.MethodName == "Alpha");
        var bravoTrace = capturedTraces.Single(t => t.MethodName == "Bravo");

        // First, assert the part that works as expected.
        alphaTrace.Caller.ShouldBe("Root", "Alpha's caller should be correctly identified as Root.");
        
        // This is the assertion that FAILS with the current logic.
        // The expected, correct behavior is that Bravo is a root-level call since the
        // original Root call had already finished.
        // The actual, buggy behavior is that Bravo's caller will be incorrectly set to Alpha.
        bravoTrace.Caller.ShouldBeNull(
            "Bravo should be a new root-level call, but was incorrectly assigned a caller due to stack corruption.");
    }

    /// <summary>
    /// Simulates a complex asynchronous workflow with parallel execution and varied
    /// completion times to ensure the call graph's parent-child relationships
    /// (the 'Caller' property) remain accurate.
    /// </summary>
    [Fact]
    public void AsyncParallelExecution_WithMixedCompletionOrder_AssignsCorrectCallers()
    {
        // Arrange
        var capturedTraces = new List<Trace>();

        // Set up the visitor to capture traces for detailed inspection.
        _mockVisitor.When(v => v.VisitTrace(Arg.Any<Trace>()))
                  .Do(callInfo => capturedTraces.Add(callInfo.Arg<Trace>()));

        // Act
        // This sequence simulates the order of events as they would be seen by the
        // ProfilingContext in the described asynchronous scenario.

        // 1. Root method starts.
        var rootId = _profilingContext.BeginMethodProfiling("Root");

        // 2. Root launches Alpha and Bravo in parallel.
        var alphaId = _profilingContext.BeginMethodProfiling("Alpha");
        var bravoId = _profilingContext.BeginMethodProfiling("Bravo");

        // 3. Bravo immediately invokes and awaits Charlie.
        var charlieId = _profilingContext.BeginMethodProfiling("Charlie");
        _profilingContext.EndMethodProfiling(charlieId); // Charlie completes.

        // 4. Bravo now launches Delta, Echo, and Foxtrot in parallel.
        var deltaId = _profilingContext.BeginMethodProfiling("Delta");
        var echoId = _profilingContext.BeginMethodProfiling("Echo");
        var foxtrotId = _profilingContext.BeginMethodProfiling("Foxtrot");

        // 5. The parallel methods complete according to their simulated duration.
        _profilingContext.EndMethodProfiling(deltaId);   // Delta finishes first (shortest).
        _profilingContext.EndMethodProfiling(foxtrotId); // Foxtrot finishes next (middle).
        _profilingContext.EndMethodProfiling(echoId);    // Echo finishes last (longest).

        // 6. Bravo can now complete, as all its children are done.
        _profilingContext.EndMethodProfiling(bravoId);

        // 7. Alpha, the long-running task, finally completes.
        _profilingContext.EndMethodProfiling(alphaId);

        // 8. Root can now complete, as Alpha and Bravo are done.
        _profilingContext.EndMethodProfiling(rootId);

        // All profiling is complete. Collect the results.
        _profilingContext.CollectTraces();

        // Assert
        // The goal is to verify that despite the chaotic completion order, the initial
        // parent-child relationships established during the 'Begin' calls are correct.
        
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(7).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();

        capturedTraces.Count.ShouldBe(7);
        
        // Retrieve each trace by name for clear, specific assertions.
        var rootTrace = capturedTraces.Single(t => t.MethodName == "Root");
        var alphaTrace = capturedTraces.Single(t => t.MethodName == "Alpha");
        var bravoTrace = capturedTraces.Single(t => t.MethodName == "Bravo");
        var charlieTrace = capturedTraces.Single(t => t.MethodName == "Charlie");
        var deltaTrace = capturedTraces.Single(t => t.MethodName == "Delta");
        var echoTrace = capturedTraces.Single(t => t.MethodName == "Echo");
        var foxtrotTrace = capturedTraces.Single(t => t.MethodName == "Foxtrot");

        // Verify the entire call graph hierarchy.
        rootTrace.Caller.ShouldBeNull("Root is the entry point and should have no caller.");
        
        alphaTrace.Caller.ShouldBe("Root", "Alpha was directly invoked by Root.");
        bravoTrace.Caller.ShouldBe("Root", "Bravo was directly invoked by Root.");

        charlieTrace.Caller.ShouldBe("Bravo", "Charlie was invoked by Bravo.");
        
        deltaTrace.Caller.ShouldBe("Bravo", "Delta was invoked in parallel by Bravo.");
        echoTrace.Caller.ShouldBe("Bravo", "Echo was invoked in parallel by Bravo.");
        foxtrotTrace.Caller.ShouldBe("Bravo", "Foxtrot was invoked in parallel by Bravo.");
    }
}
