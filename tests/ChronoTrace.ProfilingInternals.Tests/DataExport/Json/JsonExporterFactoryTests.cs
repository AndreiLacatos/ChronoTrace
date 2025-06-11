using System.Text.Json;
using ChronoTrace.ProfilingInternals.DataExport;
using ChronoTrace.ProfilingInternals.DataExport.Json;
using ChronoTrace.ProfilingInternals.Settings;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public class JsonExporterFactoryTests
{
    /// <summary>
    /// It verifies the entire flow from the factory to the final file on disk.
    /// </summary>
    [Fact]
    public void Complete_WhenCalledInSequence_CreatesCorrectJsonFileOnDiskAtDesiredLocation()
    {
        // Arrange
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"ChronoTraceTests_{Guid.NewGuid()}");
        var outputPath = Path.Combine(tempDirectory, "trace-report.json");
        var settings = new ProfilingSettings { OutputPath = outputPath };
        
        // Use the actual factory to create a fully configured exporter.
        var exporter = JsonExporterFactory.MakeJsonExporter(settings);

        var trace1 = new Trace { MethodName = "Module.A.FastMethod", ExecutionTime = TimeSpan.FromMilliseconds(417) };
        var trace2 = new Trace { MethodName = "Module.B.SlowMethod", ExecutionTime = TimeSpan.FromMilliseconds(47) };
        
        // Act
        exporter.BeginVisit();
        exporter.VisitTrace(trace1);
        exporter.VisitTrace(trace2);
        exporter.Complete();

        // Assert
        var files = Directory.GetFiles(tempDirectory);
        var createdFilePath = files.ShouldHaveSingleItem("Expected exactly one file to be created.");
        
        // 2. Verify the file name is correctly rotated.
        var fileName = Path.GetFileName(createdFilePath);
        fileName.ShouldStartWith("trace-report_");
        fileName.ShouldEndWith(".json");

        // 3. Read the file and deserialize its content to verify correctness.
        var jsonContent = File.ReadAllText(createdFilePath);
        var report = JsonSerializer.Deserialize<TimingReport>(jsonContent);

        report.ShouldNotBeNull();
        var timings = report.MethodTimings.ToArray();
        timings.Length.ShouldBe(2);

        var timing1 = timings[0];
        timing1.MethodName.ShouldBe(trace1.MethodName);
        timing1.ExecutionTime.ShouldBe(trace1.ExecutionTime);

        var timing2 = timings[1];
        timing2.MethodName.ShouldBe(trace2.MethodName);
        timing2.ExecutionTime.ShouldBe(trace2.ExecutionTime);
        
        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    /// <summary>
    /// It verifies the entire flow from the factory to the final file on disk.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("                 ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Complete_WhenCalledInSequence_CreatesCorrectJsonFileOnDiskAtDefaultLocation(string? outputPath)
    {
        // Arrange
        var settings = new ProfilingSettings { OutputPath = outputPath };
        var defaultOutputDirectory = new StaticExportDirectoryProvider().GetExportDirectory();
        var defaultFileName = new StaticJsonFileNameProvider().GetJsonFileName();
        if (Directory.Exists(defaultOutputDirectory))
        {
            Directory.Delete(defaultOutputDirectory, recursive: true);
        }

        // Use the actual factory to create a fully configured exporter.
        var exporter = JsonExporterFactory.MakeJsonExporter(settings);

        var trace1 = new Trace { MethodName = "Module.A.FastMethod", ExecutionTime = TimeSpan.FromMilliseconds(417) };
        var trace2 = new Trace { MethodName = "Module.B.SlowMethod", ExecutionTime = TimeSpan.FromMilliseconds(47) };

        // Act
        exporter.BeginVisit();
        exporter.VisitTrace(trace1);
        exporter.VisitTrace(trace2);
        exporter.Complete();

        // Assert
        var files = Directory.GetFiles(defaultOutputDirectory);
        var createdFilePath = files.ShouldHaveSingleItem("Expected exactly one file to be created.");
        
        // 2. Verify the file name is correctly rotated.
        var fileName = Path.GetFileName(createdFilePath);
        fileName.ShouldStartWith(defaultFileName.Split(".").First());
        fileName.ShouldEndWith("_000001.json");

        // 3. Read the file and deserialize its content to verify correctness.
        var jsonContent = File.ReadAllText(createdFilePath);
        var report = JsonSerializer.Deserialize<TimingReport>(jsonContent);

        report.ShouldNotBeNull();
        var timings = report.MethodTimings.ToArray();
        timings.Length.ShouldBe(2);

        var timing1 = timings[0];
        timing1.MethodName.ShouldBe(trace1.MethodName);
        timing1.ExecutionTime.ShouldBe(trace1.ExecutionTime);

        var timing2 = timings[1];
        timing2.MethodName.ShouldBe(trace2.MethodName);
        timing2.ExecutionTime.ShouldBe(trace2.ExecutionTime);
        
        if (Directory.Exists(defaultOutputDirectory))
        {
            Directory.Delete(defaultOutputDirectory, recursive: true);
        }
    }
}