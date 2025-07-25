using ChronoTrace.ProfilingInternals.Compat;
using ChronoTrace.ProfilingInternals.DataExport.FileRotation;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport.FileRotation;

public class DailyCounterFileRotationStrategyTests : IDisposable
{
    private sealed class FakeTimeProvider : ITimeProvider
    {
        private DateTimeOffset _time = new DateTimeOffset(2020, 04, 29, 13, 17, 19, TimeSpan.Zero);
        public DateTimeOffset GetLocalNow() => _time;
        public DateTimeOffset GetUtcNow() => GetLocalNow();
        internal void SetUtcNow(DateTimeOffset offset) => _time = offset;
    }

    private readonly FakeTimeProvider _timeProvider;
    private readonly DailyCounterFileRotationStrategy _strategy;
    private readonly string _tempDirectory;

    public DailyCounterFileRotationStrategyTests()
    {
        _timeProvider = new FakeTimeProvider();
        _strategy = new DailyCounterFileRotationStrategy(_timeProvider);
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"ChronoTraceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    /// <summary>
    /// Tests that the first file rotation creates a file with counter 000001.
    /// </summary>
    [Fact]
    public void RotateName_FirstFile_ShouldReturnFileWithCounter000001()
    {
        // Arrange
        var baseFileName = "traces.json";

        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        result.ShouldBe("traces_20200429_000001.json");
    }

    /// <summary>
    /// Tests that subsequent rotations increment the counter correctly.
    /// </summary>
    [Fact]
    public void RotateName_WithExistingFiles_ShouldIncrementCounter()
    {
        // Arrange
        var baseFileName = "traces.json";

        // Create existing files
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_000001.json"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_000002.json"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_000005.json"), "test");

        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        result.ShouldBe("traces_20200429_000006.json");
    }

    /// <summary>
    /// Tests rotation with different date formats.
    /// </summary>
    [Theory]
    [InlineData(2020, 4, 30, "traces_20200430_000001.json")]
    [InlineData(2020, 12, 31, "traces_20201231_000001.json")]
    [InlineData(2021, 2, 28, "traces_20210228_000001.json")]
    [InlineData(2024, 2, 29, "traces_20240229_000001.json")] // Leap year
    public void RotateName_DifferentDates_ShouldFormatDateCorrectly(int year, int month, int day, string expected)
    {
        // Arrange
        _timeProvider.SetUtcNow(new DateTimeOffset(year, month, day, 13, 17, 19, TimeSpan.Zero));
        var baseFileName = "traces.json";

        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        result.ShouldBe(expected);
    }

    /// <summary>
    /// Tests rotation with files that have no extension.
    /// </summary>
    [Fact]
    public void RotateName_FileWithoutExtension_ShouldHandleCorrectly()
    {
        // Arrange
        var baseFileName = "tracefile";

        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        result.ShouldBe("tracefile_20200429_000001");
    }

    /// <summary>
    /// Tests rotation with complex file names containing special characters.
    /// </summary>
    [Fact]
    public void RotateName_ComplexFileName_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var baseFileName = "trace-file.with.dots_and-dashes.json";

        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        result.ShouldBe("trace-file.with.dots_and-dashes_20200429_000001.json");
    }

    /// <summary>
    /// Tests that files from different dates don't interfere with counter.
    /// </summary>
    [Fact]
    public void RotateName_DifferentDates_ShouldIsolateCounters()
    {
        // Arrange
        var baseFileName = "traces.json";

        // Create files for today
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200428_000001.json"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200428_000002.json"), "test");

        // Simulate date change to tomorrow
        _timeProvider.SetUtcNow(new DateTimeOffset(2020, 04, 30, 13, 17, 19, TimeSpan.Zero));

        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        result.ShouldBe("traces_20200430_000001.json"); // Should start from 1, not 3
    }

    /// <summary>
    /// Tests behavior with null or empty parent directory.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RotateName_NullOrEmptyDirectory_ShouldUseCurrentDirectory(string? parentDirectory)
    {
        // Arrange
        var baseFileName = "traces.json";

        // Act
        var result = _strategy.RotateName(parentDirectory!, baseFileName);

        // Assert
        result.ShouldBe("traces_20200429_000001.json");
    }

    /// <summary>
    /// Tests behavior when the directory doesn't exist.
    /// </summary>
    [Fact]
    public void RotateName_NonExistentDirectory_ShouldReturnFirstCounter()
    {
        // Arrange
        var baseFileName = "traces.json";
        var nonExistentDir = Path.Combine(_tempDirectory, "nonexistent");

        // Act
        var result = _strategy.RotateName(nonExistentDir, baseFileName);

        // Assert
        result.ShouldBe("traces_20200429_000001.json");
    }

    /// <summary>
    /// Tests that counter pads with leading zeros correctly.
    /// </summary>
    [Fact]
    public void RotateName_HighCounters_ShouldPadWithLeadingZeros()
    {
        // Arrange
        var baseFileName = "traces.json";

        // Create files with high counters
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_000999.json"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_001000.json"), "test");

        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        result.ShouldBe("traces_20200429_001001.json");
    }

    /// <summary>
    /// Tests that malformed files in the directory don't break the counting logic.
    /// </summary>
    [Fact]
    public void RotateName_WithMalformedFiles_ShouldIgnoreInvalidFiles()
    {
        // Arrange
        var baseFileName = "traces.json";

        // Create valid and invalid files
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_000001.json"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_000002.json"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_invalid.json"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_.json"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_abc.json"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "other_file.json"), "test");

        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        result.ShouldBe("traces_20200429_000003.json"); // Should only count valid files
    }

    /// <summary>
    /// Tests rotation with different file extensions.
    /// </summary>
    [Theory]
    [InlineData("traces.txt", "traces_20200429_000001.txt")]
    [InlineData("traces.log", "traces_20200429_000001.log")]
    [InlineData("traces.xml", "traces_20200429_000001.xml")]
    [InlineData("traces.trace", "traces_20200429_000001.trace")]
    public void RotateName_DifferentExtensions_ShouldPreserveExtension(string baseFileName, string expected)
    {
        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        result.ShouldBe(expected);
    }

    /// <summary>
    /// Tests that case sensitivity in file names is handled correctly.
    /// </summary>
    [Fact]
    public void RotateName_CaseSensitivity_ShouldHandleCorrectly()
    {
        // Arrange
        var baseFileName = "Traces.JSON";

        // Create existing files with different casing
        File.WriteAllText(Path.Combine(_tempDirectory, "Traces_20200429_000001.JSON"), "test");
        File.WriteAllText(Path.Combine(_tempDirectory, "traces_20200429_000002.json"), "test");

        // Act
        var result = _strategy.RotateName(_tempDirectory, baseFileName);

        // Assert
        // The exact behavior depends on the file system, but it should handle both files
        result.ShouldStartWith("Traces_20200429_");
        result.ShouldEndWith(".json");
    }
}