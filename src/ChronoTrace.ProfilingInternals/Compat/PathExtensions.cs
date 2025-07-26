using System.IO;

namespace ChronoTrace.ProfilingInternals.Compat
{
    /// <summary>
    /// Compatibility layer
    /// </summary>
    internal static class PathExtensions
    {
        /// <summary>
        /// Compatibility function for <c>Path.EndsInDirectorySeparator()</c>
        /// </summary>
        /// <param name="path">Path to evaluate</param>
        /// <returns>Value indicating whether the supplied path ends in directory separator char</returns>
        internal static bool EndsInDirectorySeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path[path.Length - 1] == Path.DirectorySeparatorChar;
        }
    }
}
