using ChronoTrace.ProfilingInternals.DataExport;
using ChronoTrace.ProfilingInternals.DataExport.FileRotation;
using ChronoTrace.ProfilingInternals.DataExport.Json;
using NSubstitute;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.Json;

public partial class JsonExporterTests
{
    [Fact]
    public void Complete_WhenDirectoryProviderReturnsNull_ShouldThrowAndReleaseLock()
    {
        // Arrange
        var directoryProvider = Substitute.For<IExportDirectoryProvider>();
        directoryProvider.GetExportDirectory().Returns((string)null!); // Return a null path

        var exporter = new JsonExporter(
            directoryProvider,
            Substitute.For<IJsonFileNameProvider>(),
            Substitute.For<IFileRotationStrategy>());
        
        exporter.BeginVisit(); // Set up a valid report

        // Act & Assert
        // Path.Combine will throw on a null argument.
        Should.Throw<ArgumentNullException>(() => exporter.Complete());

        // Verify the lock was released.
        _fileSystemLock.CurrentCount.ShouldBe(1);
    }

    [Fact]
    public void Complete_WhenRotatorReturnsInvalidFileName_ShouldThrowAndReleaseLock()
    {
        // Arrange
        var directoryProvider = Substitute.For<IExportDirectoryProvider>();
        directoryProvider.GetExportDirectory().Returns(_tempDirectory);
        
        var fileNameProvider = Substitute.For<IJsonFileNameProvider>();
        fileNameProvider.GetJsonFileName().Returns("valid-name.json");

        var fileRotator = Substitute.For<IFileRotationStrategy>();
        fileRotator.RotateName(Arg.Any<string>(), Arg.Any<string>()).Returns($"{string.Concat(Enumerable.Repeat("J", 6000))}.json");
        var exporter = new JsonExporter(directoryProvider, fileNameProvider, fileRotator);
        exporter.BeginVisit();
        exporter.VisitTrace(new Trace { MethodName = "A", ExecutionTime = TimeSpan.Zero });

        // Act & Assert
        // File.WriteAllText should throw because the path contains an invalid character.
        // The specific exception can be ArgumentException, NotSupportedException, or IOException.
        Should.Throw<Exception>(() => exporter.Complete())
            .ShouldBeOfType<IOException>("Expected an ArgumentException for invalid path characters.");

        // Verify the lock was released.
        _fileSystemLock.CurrentCount.ShouldBe(1);
    }
}
