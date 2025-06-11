using System.Reflection;
using ChronoTrace.ProfilingInternals.DataExport.Json;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public partial class JsonExporterTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly SemaphoreSlim _fileSystemLock;

    public JsonExporterTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"ChronoTraceTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
        
        // Using reflection to get access to the private static lock for testing purposes.
        // This is necessary to verify that the lock is released after an exception.
        var lockField = typeof(JsonExporter).GetField("FileSystemLock", BindingFlags.NonPublic | BindingFlags.Static);
        lockField.ShouldNotBeNull("Could not find the private static FileSystemLock field via reflection.");
        _fileSystemLock = (SemaphoreSlim)lockField.GetValue(null)!;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
