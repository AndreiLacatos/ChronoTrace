using System;

namespace ChronoTrace.ProfilingInternals.Compat
{
    /// <summary>
    /// Compatibility layer
    /// </summary>
    internal static class StopwatchExtensions
    {
        /// <summary>Gets the elapsed time between two timestamps</summary>
        /// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
        /// <param name="endingTimestamp">The timestamp marking the end of the time period.</param>
        /// <returns>A <see cref="TimeSpan"/> for the elapsed time between the starting and ending timestamps.</returns>
        public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
        {
            var elapsedTicks = endingTimestamp - startingTimestamp;
            return TimeSpan.FromTicks(elapsedTicks);
        }
    }
}
