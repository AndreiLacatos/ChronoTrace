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

        return _path[.._path.LastIndexOf(Path.DirectorySeparatorChar)];
    }
}
