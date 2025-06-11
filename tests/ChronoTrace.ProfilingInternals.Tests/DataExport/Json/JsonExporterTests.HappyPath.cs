using System.Text.Json;
using ChronoTrace.ProfilingInternals.DataExport;
using ChronoTrace.ProfilingInternals.DataExport.FileRotation;
using ChronoTrace.ProfilingInternals.DataExport.Json;
using ChronoTrace.ProfilingInternals.Settings;
using NSubstitute;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public partial class JsonExporterTests 
{
    /// <summary>
    /// It uses NSubstitute to test the JsonExporter in isolation, without touching the file system.
    /// </summary>
    [Fact]
    public void Complete_WhenCalled_UsesDependenciesToConstructPathAndRotateName()
    {
        // Arrange
        const string fakeDirectory = "";
        const string fakeBaseFileName = "my-traces.json";
        const string fakeRotatedFileName = "my-traces_1.json";

        // Create substitutes (mocks) for all dependencies using NSubstitute.
        var directoryProvider = Substitute.For<IExportDirectoryProvider>();
        var fileNameProvider = Substitute.For<IJsonFileNameProvider>();
        var fileRotator = Substitute.For<IFileRotationStrategy>();

        // Configure substitute behavior. The syntax is clean and direct.
        directoryProvider.GetExportDirectory().Returns(fakeDirectory);
        fileNameProvider.GetJsonFileName().Returns(fakeBaseFileName);
        fileRotator.RotateName(Arg.Any<string>(), Arg.Any<string>()).Returns(fakeRotatedFileName);
        
        var exporter = new JsonExporter(directoryProvider, fileNameProvider, fileRotator);
        var trace = new Trace
        {
            MethodName = "Test.Method",
            ExecutionTime = TimeSpan.FromMilliseconds(479),
        };

        // Act
        exporter.BeginVisit();
        exporter.VisitTrace(trace);
        exporter.Complete();

        // Assert
        // Verify that the exporter called each dependency exactly once.
        directoryProvider.Received(1).GetExportDirectory();
        fileNameProvider.Received(1).GetJsonFileName();

        // Verify that the rotator was called with the correct values from the other providers.
        fileRotator.Received(1).RotateName(fakeDirectory, fakeBaseFileName);
    }
    
    [Fact]
    public void BeginVisit_WhenCalled_ResetsInternalTimingReport()
    {
        // Arrange
        var outputPath = Path.Combine(_tempDirectory, "state-reset-test.json");
        var settings = new ProfilingSettings { OutputPath = outputPath };
        var exporter = JsonExporterFactory.MakeJsonExporter(settings);

        var firstCycleTrace = new Trace { MethodName = "FirstCycle.Method", ExecutionTime = TimeSpan.FromMilliseconds(479) };
        var secondCycleTrace = new Trace { MethodName = "SecondCycle.Method", ExecutionTime = TimeSpan.FromMilliseconds(71) };

        // Act: First cycle
        exporter.BeginVisit();
        exporter.VisitTrace(firstCycleTrace);
        exporter.Complete();

        // Act: Second cycle on the same instance
        exporter.BeginVisit();
        exporter.VisitTrace(secondCycleTrace);
        exporter.Complete();

        // Assert
        var files = Directory.GetFiles(_tempDirectory).OrderBy(f => f).ToList();
        files.Count.ShouldBe(2, "Expected two files for two separate completion cycles.");
        
        var secondFileContent = File.ReadAllText(files[1]);
        var report = JsonSerializer.Deserialize<TimingReport>(secondFileContent);
        
        report.ShouldNotBeNull();
        var timing = report.MethodTimings.ShouldHaveSingleItem("Report should only contain traces from the second cycle.");
        timing.ExecutionTime.ShouldBe(secondCycleTrace.ExecutionTime);
    }
}
