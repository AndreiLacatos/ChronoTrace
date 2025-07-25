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

        /// <summary>
        /// Provides the current date as time in the UTC time zone
        /// </summary>
        /// <returns>Universal time</returns>
        DateTimeOffset GetUtcNow();
    }
}
