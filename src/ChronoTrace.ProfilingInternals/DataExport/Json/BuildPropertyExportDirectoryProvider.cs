using System.IO;
using ChronoTrace.ProfilingInternals.Compat;

namespace ChronoTrace.ProfilingInternals.DataExport.Json
{
    /// <summary>
    /// An internal implementation of <see cref="IExportDirectoryProvider"/> that determines
    /// the export directory from a given path string configured via a build property.
    /// </summary>
    internal sealed class BuildPropertyExportDirectoryProvider : IExportDirectoryProvider
    {
        private readonly string _path;

        internal BuildPropertyExportDirectoryProvider(string path)
        {
            _path = path;
        }

        public string GetExportDirectory()
        {
            if (PathExtensions.EndsInDirectorySeparator(_path))
            {
                return _path;
            }

            var directorySeparatorIndex = _path.LastIndexOf(Path.DirectorySeparatorChar);
            return directorySeparatorIndex > 0 ? _path.Substring(directorySeparatorIndex) : string.Empty;
        }
    }
}
