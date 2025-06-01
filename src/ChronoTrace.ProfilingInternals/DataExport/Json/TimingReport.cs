using System.Text.Json.Serialization;

namespace ChronoTrace.ProfilingInternals.DataExport.Json;

/// <summary>
/// Represents a full report containing a collection of method execution timings.
/// This class is used for serializing trace data to JSON.
/// </summary>
internal sealed class TimingReport
{
    /// <summary>
    /// Represents the performance timing data for a single instrumented method call.
    /// </summary>
    internal sealed class MethodTiming
    {
        /// <summary>
        /// Gets name of the method that was timed.
        /// </summary>
        [JsonPropertyName("methodName")]
        public required string MethodName { get; init; }

        /// <summary>
        /// Gets the duration of the method's execution.
        /// </summary>
        [JsonPropertyName("executionTime")]
        public required TimeSpan ExecutionTime { get; init; }
    }

    /// <summary>
    /// Gets the collection of individual method timing entries.
    /// </summary>
    [JsonPropertyName("timings")]
    public required ICollection<MethodTiming> MethodTimings { get; init; }
}
