using System.Text.RegularExpressions;

namespace ChronoTrace.ProfilingInternals.DataExport.FileRotation;

internal sealed class DailyCounterFileRotationStrategy : IFileRotationStrategy
{
    private const string DateFormat = "yyyyMMdd";
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _semaphore;

    internal DailyCounterFileRotationStrategy(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public string RotateName(string parentDirectory, string baseFileName)
    {
        var todayDateString = _timeProvider.GetLocalNow().ToString(DateFormat);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
        var extension = Path.GetExtension(baseFileName);

        int previousFileIndex;
        _semaphore.Wait();
        try
        {
            previousFileIndex = GetPreviousFileIndex(
                parentDirectory,
                fileNameWithoutExtension,
                extension,
                todayDateString);
        }
        finally
        {
            _semaphore.Release();
        }

        var nextIndex = (previousFileIndex + 1).ToString().PadLeft(6, '0');
        return $"{fileNameWithoutExtension}_{todayDateString}_{nextIndex}{extension}";
    }

    private static int GetPreviousFileIndex(
        string parentDirectory,
        string fileNameWithoutExtension,
        string fileExtension,
        string dateString)
    {
        parentDirectory = string.IsNullOrWhiteSpace(parentDirectory) ? Directory.GetCurrentDirectory() : parentDirectory;
        if (!Directory.Exists(parentDirectory))
        {
            return 0;
        }

        var searchPattern = $"{fileNameWithoutExtension}_{dateString}_*{fileExtension}";
        var directoryInfo = new DirectoryInfo(parentDirectory);
        var lastFile =  directoryInfo
            .EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly)
            .Select(fileInfo => fileInfo.Name)
            .OrderDescending()
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(lastFile))
        {
            return 0;
        }

        var escapedFileNameWithoutExtension = Regex.Escape(fileNameWithoutExtension);
        var regexPatternString = $"^{escapedFileNameWithoutExtension}_{dateString}_(\\d+){fileExtension}$";
        var fileCounterRegex = new Regex(regexPatternString, RegexOptions.IgnoreCase);
        var match = fileCounterRegex.Match(lastFile);

        if (match.Success && int.TryParse(match.Groups[1].Value, out var counter))
        {
            return counter;
        }

        return 0;
    }
}
