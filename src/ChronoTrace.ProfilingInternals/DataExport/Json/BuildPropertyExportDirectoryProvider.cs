namespace ChronoTrace.ProfilingInternals.DataExport.Json;

internal sealed class BuildPropertyExportDirectoryProvider : IExportDirectoryProvider
{
    private readonly string _path;

    internal BuildPropertyExportDirectoryProvider(string path)
    {
        _path = path;
    }

    public string GetExportDirectory()
    {
        if (Path.EndsInDirectorySeparator(_path))
        {
            return _path;
        }

        var directorySeparatorIndex = _path.LastIndexOf(Path.DirectorySeparatorChar);
        return directorySeparatorIndex > 0 ? _path[..directorySeparatorIndex] : string.Empty;
    }
}
