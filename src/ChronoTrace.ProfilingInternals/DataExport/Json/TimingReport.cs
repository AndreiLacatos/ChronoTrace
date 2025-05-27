using System.Text.Json.Serialization;

namespace ChronoTrace.ProfilingInternals.DataExport.Json;

internal sealed class TimingReport
{
    internal sealed class MethodTiming
    {
        [JsonPropertyName("methodName")]
        public required string MethodName { get; init; }

        [JsonPropertyName("executionTime")]
        public required TimeSpan ExecutionTime { get; init; }
    }

    [JsonPropertyName("timings")]
    public required ICollection<MethodTiming> MethodTimings { get; init; }
}