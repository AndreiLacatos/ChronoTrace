using System;

namespace ChronoTrace.ProfilingInternals.Compat
{
    /// <inheritdoc/>
    internal sealed class TimeProvider : ITimeProvider
    {
        /// <inheritdoc/>
        public DateTimeOffset GetLocalNow()
        {
            return DateTimeOffset.Now;
        }
    }
}
