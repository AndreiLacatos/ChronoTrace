using System;
using ChronoTrace.ProfilingInternals.Protection;

namespace ChronoTrace.ProfilingInternals.Compat
{
    /// <inheritdoc/>
    [LibraryUsage]
    public sealed class TimeProvider : ITimeProvider
    {
        /// <inheritdoc/>
        public DateTimeOffset GetLocalNow()
        {
            return DateTimeOffset.Now;
        }
    }
}
