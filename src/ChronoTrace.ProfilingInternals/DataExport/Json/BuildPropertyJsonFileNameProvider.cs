namespace ChronoTrace.ProfilingInternals.DataExport.Json;

/// <summary>
/// An internal implementation of <see cref="IJsonFileNameProvider"/> that extracts
/// the JSON file name from the full path string configured via a build property.
/// </summary>
internal sealed class BuildPropertyJsonFileNameProvider : IJsonFileNameProvider
{
    private readonly string _path;

    internal BuildPropertyJsonFileNameProvider(string path)
    {
        _path = path;
    }

    public string GetJsonFileName()
    {
        return Path.GetFileName(_path);
    }
}
