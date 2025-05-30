namespace ChronoTrace.ProfilingInternals.DataExport.Json;

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
