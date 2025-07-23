using System.Collections.Concurrent;
using System.Text.Json;
using ChronoTrace.ProfilingInternals.DataExport;
using ChronoTrace.ProfilingInternals.DataExport.Json;
using ChronoTrace.ProfilingInternals.Settings.DataExport;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public partial class JsonExporterTests
{
    [Fact]
    public async Task Complete_WhenCalledByMultipleThreadsConcurrently_ShouldSerializeAccessAndWriteAllFiles()
    {
        // Arrange
        const int numberOfThreads = 10;
        var exceptions = new ConcurrentBag<Exception>();
        var tasks = new List<Task>();

        // We use a real file rotator to ensure file names are unique and reflect serialized access.
        for (var i = 0; i < numberOfThreads; i++)
        {
            // Each thread gets its own exporter instance and its own unique data.
            var threadId = i;
            var settings = new JsonExporterSettings { OutputPath = Path.Combine(_tempDirectory, "trace.json") };

            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var exporter = JsonExporterFactory.MakeJsonExporter(settings);
                    exporter.BeginVisit();
                    exporter.VisitTrace(new Trace
                    {
                        MethodName = $"Method_From_Thread_{threadId}",
                        ExecutionTime = TimeSpan.FromMilliseconds(100 + threadId),
                    });
                    exporter.Complete();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        // Act
        await Task.WhenAll(tasks);

        // Assert
        // 1. Verify that no exceptions were thrown during execution (like file-in-use).
        exceptions.ShouldBeEmpty("No exceptions should have been thrown by any thread.");

        // 2. Verify that a file was created for each thread.
        var createdFiles = Directory.GetFiles(_tempDirectory);
        createdFiles.Length.ShouldBe(numberOfThreads, $"Expected {numberOfThreads} files to be created.");

        // 3. Verify the content of each file to ensure no data was corrupted or lost.
        var foundMethodNames = new HashSet<string>();
        foreach (var filePath in createdFiles)
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var report = JsonSerializer.Deserialize<TimingReport>(jsonContent);
            
            report.ShouldNotBeNull();
            var timing = report.MethodTimings.ShouldHaveSingleItem();
            
            // Add the method name to a set to ensure each thread's data is present and unique.
            foundMethodNames.Add(timing.MethodName);
        }

        // 4. Final check: ensure all unique method names were found across all files.
        foundMethodNames.Count.ShouldBe(numberOfThreads, "All unique method names from each thread should be present in the output files.");
    }
    
    [Fact]
    public async Task Complete_WhenOneThreadFails_ShouldReleaseLockAndNotBlockOtherThreads()
    {
        // Arrange
        const int exportCount = 20;
        var exceptions = new ConcurrentBag<Exception>();

        // Create the failing exporter first.
        var failingSettings = new JsonExporterSettings
        {
            OutputPath = Path.Combine(_tempDirectory, $"{string.Concat(Enumerable.Repeat("J", 6000))}.json"),
        };

        var tasks = Enumerable.Range(0, exportCount)
            .Select(_ => Task.Run(() =>
            {
                try
                {
                    var failingExporter = JsonExporterFactory.MakeJsonExporter(failingSettings);
                    failingExporter.BeginVisit();
                    failingExporter.VisitTrace(new Trace { MethodName = "ThisWillFail", ExecutionTime = TimeSpan.Zero });
                    failingExporter.Complete();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }))
            .ToList();

        // Act
        await Task.WhenAll(tasks);

        // Assert
        // 1. Verify that exactly one exception occurred (from the failing thread).
        exceptions.Count.ShouldBe(exportCount);
        exceptions.ShouldAllBe(e => e is IOException);

        // 3. Verify the lock was ultimately released and is available.
        _fileSystemLock.CurrentCount.ShouldBe(1, "The lock must be released even after a thread fails.");
    }
}
