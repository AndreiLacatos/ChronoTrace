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
}
