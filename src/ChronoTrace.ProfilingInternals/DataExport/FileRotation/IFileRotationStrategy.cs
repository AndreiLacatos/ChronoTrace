namespace ChronoTrace.ProfilingInternals.DataExport.FileRotation
{
    internal interface IFileRotationStrategy
    {
        string RotateName(string parentDirectory, string baseFileName);
    }
}
