using System.Text.RegularExpressions;

namespace ChronoTrace.ProfilingInternals.DataExport.FileRotation;

internal sealed class DailyCounterFileRotationStrategy : IFileRotationStrategy
{
    private const string DateFormat = "yyyyMMdd";
    private readonly TimeProvider _timeProvider;

    internal DailyCounterFileRotationStrategy(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public string RotateName(string parentDirectory, string baseFileName)
    {
        var todayDateString = _timeProvider.GetLocalNow().ToString(DateFormat);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
        var extension = Path.GetExtension(baseFileName);

        var previousFileIndex = GetPreviousFileIndex(
            parentDirectory,
            fileNameWithoutExtension,
            extension,
            todayDateString);
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

        var escapedFileNameWithoutExtension = Regex.Escape(fileNameWithoutExtension);
        var regexPatternString = $"^{escapedFileNameWithoutExtension}_{dateString}_(\\d{{6}}){fileExtension}$";
        var fileCounterRegex = new Regex(regexPatternString, RegexOptions.IgnoreCase);
        
        
        // list files that look like trace files
        var searchPattern = $"{fileNameWithoutExtension}_{dateString}_*{fileExtension}";
        var directoryInfo = new DirectoryInfo(parentDirectory);
        var candidateFiles =  directoryInfo
            .EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly)
            .Select(fileInfo => fileInfo.Name);
        
        // filter out any noise
        var matchingFiles = candidateFiles
            .Select(name => fileCounterRegex.Match(name))
            .Where(match => match.Success)
            .ToArray();

        if (matchingFiles.Length == 0)
        {
            return 0;
        }

        return matchingFiles
            .Select(match => int.Parse(match.Groups[1].Value))
            .Max();
    }
}
