using ChronoTrace.ProfilingInternals.DataExport;
using ChronoTrace.ProfilingInternals.DataExport.Stdout;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Stdout;

public class StdoutExporterTests
{
    /// <summary>
    /// Verifies a happy-path of the stdout trace exporter
    /// </summary>
    [Theory]
    [InlineData(0, "00:00.000")]
    [InlineData(1234, "00:01.234")]
    [InlineData(59999, "00:59.999")]
    [InlineData(60000, "01:00.000")]
    [InlineData(3599999, "59:59.999")]
    [InlineData(3600000, "60:00.000")]
    [InlineData(3660123, "61:00.123")]
    [InlineData(123456789, "2057:36.789")]
    public void Complete_WhenCalled_ShouldOutputTracesInExpectedFormat(
        int executionMilliseconds,
        string expectedFormattedDuration)
    {
        // Arrange
        var exporter = new StdoutExporter(new CallGraphBuilder());
        var trace = new Trace
        {
            MethodName = "Test.Method",
            ExecutionTime = TimeSpan.FromMilliseconds(executionMilliseconds),
            Caller = null,
        };
        var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        // Act
        exporter.BeginVisit();
        exporter.VisitTrace(trace);
        exporter.Complete();

        // Assert
        var output = sw.ToString().Trim();
        output.ShouldBe($"Test.Method: {expectedFormattedDuration}");

        // Cleanup
        Console.SetOut(originalOut);
    }

    /// <summary>
    /// Verifies a happy-path of the stdout trace exporter from the perspective of output indentation
    /// </summary>
    [Fact]
    public void Complete_WhenCalled_ShouldOutputTracesInExpectedIndentation()
    {
        // Arrange
        var exporter = new StdoutExporter(new CallGraphBuilder());
        var traces = new List<Trace>
        {
            new Trace
            {
                MethodName = "Test.Method",
                ExecutionTime = TimeSpan.FromMilliseconds(120),
                Caller = null,
            },
            new Trace
            {
                MethodName = "Test.Alpha",
                ExecutionTime = TimeSpan.FromMilliseconds(120),
                Caller = "Test.Method",
            },
            new Trace
            {
                MethodName = "Test.Bravo",
                ExecutionTime = TimeSpan.FromMilliseconds(120),
                Caller = "Test.Alpha",
            },
            new Trace
            {
                MethodName = "Test.Charlie",
                ExecutionTime = TimeSpan.FromMilliseconds(120),
                Caller = "Test.Method",
            },
        };
        var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        // Act
        exporter.BeginVisit();
        foreach (var trace in traces)
        {
            exporter.VisitTrace(trace);
        }
        exporter.Complete();

        // Assert
        var output = sw.ToString().Trim();
        output.ShouldBe(
            """
            Test.Method: 00:00.120
                Test.Alpha: 00:00.120
                    Test.Bravo: 00:00.120
                Test.Charlie: 00:00.120
            """);

        // Cleanup
        Console.SetOut(originalOut);
    }
}
