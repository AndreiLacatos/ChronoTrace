using System;

namespace ChronoTrace.ProfilingInternals.Compat
{
    /// <summary>
    /// Compatibility layer for <c>TimeProvider</c> available in newer .NET versions
    /// </summary>
    internal interface ITimeProvider
    {
        /// <summary>
        /// Provides the current date as time in the local time zone
        /// </summary>
        /// <returns>Local time</returns>
        DateTimeOffset GetLocalNow();
    }
}