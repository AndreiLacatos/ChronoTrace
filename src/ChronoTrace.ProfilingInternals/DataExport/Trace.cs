using System;

namespace ChronoTrace.ProfilingInternals.DataExport
{
    /// <summary>
    /// Encapsulate a single trace collected during runtime.
    /// </summary>
    internal struct Trace
    {
        /// <summary>
        /// Name of the traced method.
        /// </summary>
        internal string MethodName { get; set; }

        /// <summary>
        /// Measured execution time.
        /// </summary>
        internal TimeSpan ExecutionTime { get; set; }
    }
}
