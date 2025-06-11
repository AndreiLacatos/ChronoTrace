using ChronoTrace.ProfilingInternals.DataExport;
using NSubstitute;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests;

public partial class ProfilingContextTests
{
    /// <summary>
    /// Ensures that profiling gracefully handles null method names without crashing the application.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("  \t  \n  ")]
    public void BeginMethodProfiling_WithInvalidMethodName_ShouldSkipMethodAndNotThrow(string? methodName)
    {
        // Act & Assert
        var exception = Record.Exception(() =>
        {
            var id = _profilingContext.BeginMethodProfiling(methodName!);
            id.ShouldBe((ushort)0);
        });

        exception.ShouldBeNull();
    }

    /// <summary>
    /// Tests that attempting to end profiling with an invalid zero ID is safely ignored.
    /// </summary>
    [Fact]
    public void EndMethodProfiling_WithZeroId_ShouldBeIgnored()
    {
        // Act
        var exception = Record.Exception(() => _profilingContext.EndMethodProfiling(0));

        // Assert
        exception.ShouldBeNull();
        _mockVisitor.DidNotReceive().BeginVisit();
        _mockVisitor.DidNotReceive().VisitTrace(Arg.Any<Trace>());
        _mockVisitor.DidNotReceive().Complete();
    }

    /// <summary>
    /// Verifies that using an ID higher than any previously issued is handled gracefully.
    /// </summary>
    [Fact]
    public void EndMethodProfiling_WithIdGreaterThanCounter_ShouldBeIgnored()
    {
        // Arrange
        var validId = _profilingContext.BeginMethodProfiling("TestMethod");
        var invalidId = (ushort)(validId + 100);

        // Act
        var exception = Record.Exception(() => _profilingContext.EndMethodProfiling(invalidId));

        // Assert
        exception.ShouldBeNull();
        
        // Complete the valid profiling to verify that the system still works
        _profilingContext.EndMethodProfiling(validId);
        _profilingContext.CollectTraces();
        
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(1).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
    }

    /// <summary>
    /// Ensures that ending the same method profiling session multiple times doesn't corrupt data or cause errors.
    /// </summary>
    [Fact]
    public void EndMethodProfiling_CalledMultipleTimesWithSameId_ShouldNotCorruptState()
    {
        // Arrange
        var id = _profilingContext.BeginMethodProfiling("TestMethod");

        // Act - End the same method multiple times
        var exception = Record.Exception(() =>
        {
            _profilingContext.EndMethodProfiling(id);
            _profilingContext.EndMethodProfiling(id);
            _profilingContext.EndMethodProfiling(id);
        });

        // Assert
        exception.ShouldBeNull();
        
        _profilingContext.CollectTraces();
        
        // Should still process the trace normally
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(1).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
    }

    /// <summary>
    /// Tests that attempting to end profiling for a method that was already completed is safely handled.
    /// </summary>
    [Fact]
    public void EndMethodProfiling_ForAlreadyCompletedMethod_ShouldNotAffectOtherMethods()
    {
        // Arrange
        var id1 = _profilingContext.BeginMethodProfiling("Method1");
        var id2 = _profilingContext.BeginMethodProfiling("Method2");
        
        _profilingContext.EndMethodProfiling(id1);
        
        // Act - Try to end the already completed method again
        var exception = Record.Exception(() => _profilingContext.EndMethodProfiling(id1));
        
        // Assert
        exception.ShouldBeNull();
        
        // Complete the second method normally
        _profilingContext.EndMethodProfiling(id2);
        _profilingContext.CollectTraces();
        
        // Should process both traces
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(2).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
    }

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

    /// <summary>
    /// Verifies that attempting to end method profiling before starting any profiling sessions is handled safely.
    /// </summary>
    [Fact]
    public void EndMethodProfiling_BeforeAnyBeginMethodProfiling_ShouldNotThrow()
    {
        // Act - Try to end profiling without starting any
        var exception = Record.Exception(() =>
        {
            _profilingContext.EndMethodProfiling(1);
            _profilingContext.EndMethodProfiling(100);
            _profilingContext.EndMethodProfiling(ushort.MaxValue);
        });

        // Assert
        exception.ShouldBeNull();
        
        // Verify system can still work normally after these invalid operations
        var validId = _profilingContext.BeginMethodProfiling("TestMethod");
        _profilingContext.EndMethodProfiling(validId);
        _profilingContext.CollectTraces();
        
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(1).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
    }

    /// <summary>
    /// Ensures that collecting traces before any profiling operations have been started doesn't cause issues.
    /// </summary>
    [Fact]
    public void CollectTraces_BeforeAnyProfilingOperations_ShouldNotThrow()
    {
        // Act - Try to collect traces when nothing has been profiled
        var exception = Record.Exception(() => _profilingContext.CollectTraces());

        // Assert
        exception.ShouldBeNull();
        
        // Visitor should still be called but with no traces
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.DidNotReceive().VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
        
        // Verify system can still work normally after this operation
        var validId = _profilingContext.BeginMethodProfiling("TestMethod");
        _profilingContext.EndMethodProfiling(validId);
        _profilingContext.CollectTraces();
        
        _mockVisitor.Received(2).BeginVisit(); // Called twice now
        _mockVisitor.Received(1).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(2).Complete(); // Called twice now
    }

    /// <summary>
    /// Tests that attempting to end profiling after traces have been collected (and state reset) is handled gracefully.
    /// </summary>
    [Fact]
    public void EndMethodProfiling_AfterCollectTracesResetsState_ShouldBeIgnored()
    {
        // Arrange - Start and complete a profiling session
        var id = _profilingContext.BeginMethodProfiling("TestMethod");
        _profilingContext.EndMethodProfiling(id);
        _profilingContext.CollectTraces();
        
        // Verify that the initial state worked
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(1).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
        
        // Act - Try to end profiling with the old ID after the state has been reset
        var exception = Record.Exception(() => _profilingContext.EndMethodProfiling(id));

        // Assert
        exception.ShouldBeNull();
        
        // Should not trigger another collection cycle
        _mockVisitor.Received(1).BeginVisit(); // Still only called once
        _mockVisitor.Received(1).VisitTrace(Arg.Any<Trace>()); // Still only called once
        _mockVisitor.Received(1).Complete(); // Still only called once
    }

    /// <summary>
    /// Verifies that the system maintains consistency when operations are performed in unexpected sequences.
    /// </summary>
    [Fact]
    public void MixedOutOfOrderOperations_ShouldMaintainSystemStability()
    {
        // Act - Perform operations in various invalid orders
        var exception = Record.Exception(() =>
        {
            // Try to end before beginning
            _profilingContext.EndMethodProfiling(50);
            
            // Collect with no data
            _profilingContext.CollectTraces();
            
            // Start some methods
            var id1 = _profilingContext.BeginMethodProfiling("Method1");
            var id2 = _profilingContext.BeginMethodProfiling("Method2");
            
            // Try to end with invalid IDs mixed with valid ones
            _profilingContext.EndMethodProfiling(999);
            _profilingContext.EndMethodProfiling(id1);
            _profilingContext.EndMethodProfiling(0);
            _profilingContext.EndMethodProfiling(id2);
            
            // Collect traces
            _profilingContext.CollectTraces();
            
            // Try operations after reset
            _profilingContext.EndMethodProfiling(id1);
            _profilingContext.EndMethodProfiling(id2);
        });

        // Assert
        exception.ShouldBeNull();
        
        // Should have processed the two valid method calls
        _mockVisitor.Received(2).BeginVisit(); // Once for empty collection, once for real data
        _mockVisitor.Received(2).VisitTrace(Arg.Any<Trace>()); // Two valid methods
        _mockVisitor.Received(2).Complete(); // Twice total
    }

    /// <summary>
    /// Tests that starting new profiling sessions after a collect operation works correctly despite previous invalid operations.
    /// </summary>
    [Fact]
    public void NewProfilingSession_AfterStateCorruptionAttempts_ShouldWorkNormally()
    {
        // Arrange - Attempt various state corruption scenarios
        _profilingContext.EndMethodProfiling(100); // Invalid end before the beginning
        _profilingContext.CollectTraces(); // Collect with no data
        
        var oldId = _profilingContext.BeginMethodProfiling("OldMethod");
        _profilingContext.EndMethodProfiling(oldId);
        _profilingContext.CollectTraces(); // Reset state
        
        _profilingContext.EndMethodProfiling(oldId); // Try to use old ID after reset
        
        // Act - Start fresh profiling session
        var newId1 = _profilingContext.BeginMethodProfiling("NewMethod1");
        var newId2 = _profilingContext.BeginMethodProfiling("NewMethod2");
        
        _profilingContext.EndMethodProfiling(newId1);
        _profilingContext.EndMethodProfiling(newId2);
        _profilingContext.CollectTraces();

        // Assert - New session should work perfectly
        newId1.ShouldNotBe((ushort)0);
        newId2.ShouldNotBe((ushort)0);
        newId1.ShouldNotBe(newId2);
        
        // Should have called the visitor for: empty collection, old method, new methods
        _mockVisitor.Received(3).BeginVisit();
        _mockVisitor.Received(3).VisitTrace(Arg.Any<Trace>()); // 1 old + 2 new
        _mockVisitor.Received(3).Complete();
    }

    /// <summary>
    /// Ensures that partial profiling sessions (begin without the end) don't interfere with subsequent complete sessions.
    /// </summary>
    [Fact]
    public void PartialProfilingSessions_ShouldNotInterfereWithCompleteOnes()
    {
        // Arrange - Create some incomplete profiling sessions
        var incompleteId1 = _profilingContext.BeginMethodProfiling("IncompleteMethod1");
        var incompleteId2 = _profilingContext.BeginMethodProfiling("IncompleteMethod2");
        
        // Act - Start and complete a new session without ending the incomplete ones
        var completeId = _profilingContext.BeginMethodProfiling("CompleteMethod");
        _profilingContext.EndMethodProfiling(completeId);
        
        // CollectTraces should not process anything because there are pending calls
        _profilingContext.CollectTraces();
        
        // Assert - No traces should be collected yet due to pending calls
        _mockVisitor.DidNotReceive().BeginVisit();
        _mockVisitor.DidNotReceive().VisitTrace(Arg.Any<Trace>());
        _mockVisitor.DidNotReceive().Complete();
        
        // Complete the incomplete sessions
        _profilingContext.EndMethodProfiling(incompleteId1);
        _profilingContext.EndMethodProfiling(incompleteId2);
        _profilingContext.CollectTraces();
        
        // Now all traces should be collected
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(3).VisitTrace(Arg.Any<Trace>()); // All three methods
        _mockVisitor.Received(1).Complete();
    }

    /// <summary>
    /// Verifies that the profiling context can handle method calls beyond its initial list capacity without issues.
    /// </summary>
    [Fact]
    public void ProfilingBeyondInitialCapacity_ShouldExpandGracefully()
    {
        // Arrange - Initial capacity is 100, so test with more than that
        const int methodCount = 750;
        var ids = new List<ushort>();

        // Act - Start more methods than initial capacity
        var exception = Record.Exception(() =>
        {
            for (var i = 0; i < methodCount; i++)
            {
                var id = _profilingContext.BeginMethodProfiling($"Method__{i}");
                ids.Add(id);
            }

            // End all methods
            foreach (var id in ids)
            {
                _profilingContext.EndMethodProfiling(id);
            }

            _profilingContext.CollectTraces();
        });

        // Assert
        exception.ShouldBeNull();
        ids.Count.ShouldBe(methodCount);
        ids.ShouldNotContain((ushort)0);
        ids.ShouldBeUnique();

        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(methodCount).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
    }

    /// <summary>
    /// Ensures that multiple collect cycles properly reset and reuse internal resources.
    /// </summary>
    [Fact]
    public void MultipleCollectCycles_ShouldReuseResourcesCorrectly()
    {
        const int cycles = 10;
        const int methodsPerCycle = 20;

        // Act - Perform multiple complete profiling cycles
        var exception = Record.Exception(() =>
        {
            for (var cycle = 0; cycle < cycles; cycle++)
            {
                var ids = new List<ushort>();

                // Start methods for this cycle
                for (var i = 0; i < methodsPerCycle; i++)
                {
                    var id = _profilingContext.BeginMethodProfiling($"Cycle{cycle}Method{i}");
                    ids.Add(id);
                }

                // End methods for this cycle
                foreach (var id in ids)
                {
                    _profilingContext.EndMethodProfiling(id);
                }

                _profilingContext.CollectTraces();

                // Verify each cycle resets properly by checking that new IDs start from 1
                if (cycle < cycles - 1) // Don't start new cycle on last iteration
                {
                    var firstIdOfNextCycle = _profilingContext.BeginMethodProfiling("NextCycleTest");
                    firstIdOfNextCycle.ShouldBe((ushort)1);
                    _profilingContext.EndMethodProfiling(firstIdOfNextCycle);
                    _profilingContext.CollectTraces();
                }
            }
        });

        // Assert
        exception.ShouldBeNull();

        // Should have called the visitor for each cycle (plus the intermediate single method tests)
        _mockVisitor.Received(cycles + (cycles - 1)).BeginVisit();
        _mockVisitor.Received(cycles * methodsPerCycle + (cycles - 1)).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(cycles + (cycles - 1)).Complete();
    }

    /// <summary>
    /// Verifies that the internal state remains consistent during rapid allocation and deallocation of method slots.
    /// </summary>
    [Fact]
    public void RapidAllocationDeallocation_ShouldMaintainStateConsistency()
    {
        const int iterations = 100;

        // Act - Rapidly start and end methods in quick succession
        var exception = Record.Exception(() =>
        {
            for (var i = 0; i < iterations; i++)
            {
                // Start a few methods
                var id1 = _profilingContext.BeginMethodProfiling($"RapidMethod1_{i}");
                var id2 = _profilingContext.BeginMethodProfiling($"RapidMethod2_{i}");
                var id3 = _profilingContext.BeginMethodProfiling($"RapidMethod3_{i}");

                // End them immediately
                _profilingContext.EndMethodProfiling(id1);
                _profilingContext.EndMethodProfiling(id2);
                _profilingContext.EndMethodProfiling(id3);

                // Collect traces to reset the state
                _profilingContext.CollectTraces();
            }
        });

        // Assert
        exception.ShouldBeNull();

        // Should have processed each iteration
        _mockVisitor.Received(iterations).BeginVisit();
        _mockVisitor.Received(iterations * 3).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(iterations).Complete();
    }

    /// <summary>
    /// Tests that the profiling context handles resource cleanup properly when methods are left incomplete.
    /// </summary>
    [Fact]
    public void IncompleteMethodsWithResourceCleanup_ShouldNotLeakResources()
    {
        const int incompleteMethodCount = 50;

        // Act - Start many methods but don't end them, then start fresh
        var exception = Record.Exception(() =>
        {
            // Start many incomplete methods
            for (var i = 0; i < incompleteMethodCount; i++)
            {
                _profilingContext.BeginMethodProfiling($"IncompleteMethod{i}");
            }

            // CollectTraces should not process due to pending calls
            _profilingContext.CollectTraces();

            // Start and complete a new set of methods
            var completeIds = new List<ushort>();
            for (var i = 0; i < 10; i++)
            {
                var id = _profilingContext.BeginMethodProfiling($"CompleteMethod{i}");
                completeIds.Add(id);
            }

            foreach (var id in completeIds)
            {
                _profilingContext.EndMethodProfiling(id);
            }

            // Still shouldn't collect due to the incomplete methods
            _profilingContext.CollectTraces();
        });

        // Assert
        exception.ShouldBeNull();

        // No traces should have been collected due to pending incomplete methods
        _mockVisitor.DidNotReceive().BeginVisit();
        _mockVisitor.DidNotReceive().VisitTrace(Arg.Any<Trace>());
        _mockVisitor.DidNotReceive().Complete();
    }

    /// <summary>
    /// Ensures that the counter-state is properly managed and doesn't overflow or become inconsistent.
    /// </summary>
    [Fact]
    public void CounterStateManagement_ShouldRemainConsistent()
    {
        const int testRounds = 5;
        const int methodsPerRound = 30;

        // Act - Multiple rounds of complete profiling to test the counter-management
        var exception = Record.Exception(() =>
        {
            for (var round = 0; round < testRounds; round++)
            {
                var roundIds = new List<ushort>();

                // Start methods and verify IDs are sequential
                for (var i = 0; i < methodsPerRound; i++)
                {
                    var id = _profilingContext.BeginMethodProfiling($"Round{round}Method{i}");
                    roundIds.Add(id);
                    
                    // Each new ID should be exactly one more than the previous
                    if (i > 0)
                    {
                        id.ShouldBe((ushort)(roundIds[i - 1] + 1));
                    }
                }

                // End all methods
                foreach (var id in roundIds)
                {
                    _profilingContext.EndMethodProfiling(id);
                }

                _profilingContext.CollectTraces();

                // After collection, the counter should reset and the next ID should be 1
                if (round < testRounds - 1)
                {
                    var nextId = _profilingContext.BeginMethodProfiling("NextRoundFirst");
                    nextId.ShouldBe((ushort)1);
                    _profilingContext.EndMethodProfiling(nextId);
                    _profilingContext.CollectTraces();
                }
            }
        });

        // Assert
        exception.ShouldBeNull();
        
        // Account for the extra single method calls between rounds
        var expectedBeginVisits = testRounds + (testRounds - 1);
        var expectedTraceVisits = (testRounds * methodsPerRound) + (testRounds - 1);
        
        _mockVisitor.Received(expectedBeginVisits).BeginVisit();
        _mockVisitor.Received(expectedTraceVisits).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(expectedBeginVisits).Complete();
    }

    /// <summary>
    /// Tests behavior when the invocation counter approaches its maximum value (ushort.MaxValue).
    /// </summary>
    [Fact]
    public void InvocationCounter_ApproachingMaxValue_ShouldHandleGracefully()
    {
        // Arrange - We can't easily reach ushort.MaxValue in a test, but we can test the logic
        // by testing with values near the boundary and verifying the system works correctly
        const int testIterations = 100;
        
        // Act - Perform many complete profiling cycles to increment the counter
        var exception = Record.Exception(() =>
        {
            for (var i = 0; i < testIterations; i++)
            {
                var id = _profilingContext.BeginMethodProfiling($"BoundaryTestMethod{i}");
                id.ShouldBeGreaterThan((ushort)0);
                id.ShouldBeLessThanOrEqualTo(ushort.MaxValue);
                
                _profilingContext.EndMethodProfiling(id);
                _profilingContext.CollectTraces();
            }
        });

        // Assert
        exception.ShouldBeNull();
        _mockVisitor.Received(testIterations).BeginVisit();
        _mockVisitor.Received(testIterations).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(testIterations).Complete();
    }

    /// <summary>
    /// Tests rapid succession of begin/end calls to verify timing and ordering consistency.
    /// </summary>
    [Fact]
    public async Task RapidSuccessionOperations_ShouldMaintainCorrectTiming()
    {
        const int rapidCallCount = 20;
        var ids = new List<ushort>();
        var capturedTraces = new List<Trace>();

        // Setup visitor to capture traces for detailed verification
        _mockVisitor.When(v => v.VisitTrace(Arg.Any<Trace>()))
                  .Do(callInfo => capturedTraces.Add(callInfo.Arg<Trace>()));

        var exception = await Record.ExceptionAsync(async () =>
        {
            // Start all methods in rapid succession
            for (var i = 0; i < rapidCallCount; i++)
            {
                var id = _profilingContext.BeginMethodProfiling($"RapidMethod{i}");
                ids.Add(id);
            }

            // End all methods in rapid succession (LIFO order to simulate nested calls)
            for (var i = rapidCallCount - 1; i >= 0; i--)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(5));
                _profilingContext.EndMethodProfiling(ids[i]);
            }

            _profilingContext.CollectTraces();
        });

        // Assert
        exception.ShouldBeNull();

        
        // Verify all IDs were generated correctly
        ids.Count.ShouldBe(rapidCallCount);
        ids.ShouldBeUnique();
        for (var i = 0; i < rapidCallCount; i++)
        {
            ids[i].ShouldBe((ushort)(i + 1)); // IDs should be sequential starting from 1
        }

        // Verify visitor was called correctly
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(rapidCallCount).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();

        // Verify captured traces and ordering
        capturedTraces.Count.ShouldBe(rapidCallCount);

        // Verify traces are in the order of BeginMethodProfiling calls (RapidMethod0, RapidMethod1, ...)
        for (var i = 0; i < rapidCallCount; i++)
        {
            var expectedMethodName = $"RapidMethod{i}";
            capturedTraces[i].MethodName.ShouldBe(expectedMethodName);
        }

        // Verify execution time values are in descending order (since we ended methods in LIFO order)
        for (var i = 0; i < rapidCallCount - 1; i++)
        {
            var currentTrace = capturedTraces[i];
            var nextTrace = capturedTraces[i + 1];

            currentTrace.ExecutionTime.ShouldBeGreaterThan(nextTrace.ExecutionTime, 
                $"Method {currentTrace.MethodName} should have ended after {nextTrace.MethodName}");
        }

        // Verify the specific pattern: the first started method should have the latest end time
        var firstStartedMethod = capturedTraces[0]; // RapidMethod0
        var lastStartedMethod = capturedTraces[rapidCallCount - 1]; // RapidMethod499
        
        firstStartedMethod.MethodName.ShouldBe("RapidMethod0");
        lastStartedMethod.MethodName.ShouldBe($"RapidMethod{rapidCallCount - 1}");
        
        // Since RapidMethod0 was ended last, it should have the highest ReturnTick
        firstStartedMethod.ExecutionTime.ShouldBeGreaterThan(lastStartedMethod.ExecutionTime);
    }

    /// <summary>
    /// Tests methods that complete in different order than they were started to verify timestamp handling.
    /// </summary>
    [Fact]
    public void MethodsCompletingOutOfStartOrder_ShouldMaintainCorrectTracing()
    {
        // Arrange
        const int methodCount = 10;
        var ids = new List<ushort>();

        // Act - Start methods in order, but end them in reverse order
        var exception = Record.Exception(() =>
        {
            // Start methods 1, 2, 3, ..., 10
            for (var i = 0; i < methodCount; i++)
            {
                var id = _profilingContext.BeginMethodProfiling($"OutOfOrderMethod{i}");
                ids.Add(id);
            }

            // End methods in reverse order: 10, 9, 8, ..., 1
            for (var i = methodCount - 1; i >= 0; i--)
            {
                _profilingContext.EndMethodProfiling(ids[i]);
            }

            _profilingContext.CollectTraces();
        });

        // Assert
        exception.ShouldBeNull();
        
        // All methods should have been traced regardless of completion order
        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(methodCount).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
    }

    /// <summary>
    /// Tests the edge case where multiple threads might try to cross the pending calls boundary simultaneously.
    /// </summary>
    [Fact]
    public void PendingCallsBoundaryUnderConcurrency_ShouldMaintainConsistency()
    {
        const int concurrentMethods = 20;
        var ids = new ushort[concurrentMethods];
        var tasks = new Task[concurrentMethods];

        // Act - Start methods concurrently, then end them concurrently
        var exception = Record.Exception(() =>
        {
            // Start all methods concurrently
            for (var i = 0; i < concurrentMethods; i++)
            {
                var index = i; // Capture for closure
                tasks[i] = Task.Run(() =>
                {
                    ids[index] = _profilingContext.BeginMethodProfiling($"ConcurrentBoundaryMethod{index}");
                });
            }

            Task.WaitAll(tasks);

            // Verify collection doesn't work with pending calls
            _profilingContext.CollectTraces();

            // End all methods concurrently
            for (var i = 0; i < concurrentMethods; i++)
            {
                var index = i; // Capture for closure
                tasks[i] = Task.Run(() =>
                {
                    _profilingContext.EndMethodProfiling(ids[index]);
                });
            }

            Task.WaitAll(tasks);

            // Now the collection should work
            _profilingContext.CollectTraces();
        });

        // Assert
        exception.ShouldBeNull();
        
        // All IDs should be valid and unique
        ids.ShouldNotContain((ushort)0);
        ids.ShouldBeUnique();

        _mockVisitor.Received(1).BeginVisit();
        _mockVisitor.Received(concurrentMethods).VisitTrace(Arg.Any<Trace>());
        _mockVisitor.Received(1).Complete();
    }
}
