using System.Diagnostics;

namespace ChronoTrace.ProfilingInternals
{
    /// <summary>
    /// Represents the state and timing information for a single invocation
    /// of a method being profiled. Instances of this class are created internally
    /// by the <see cref="ProfilingContext"/>.
    /// </summary>
    internal sealed class ProfiledMethodInvocation
    {
        /// <summary>
        /// Gets the unique identifier for this specific method invocation within a profiling session.
        /// </summary>
        internal ushort Id { get; set; }
        
        /// <summary>
        /// Gets the name of the method that was invoked.
        /// </summary>
        internal string MethodName { get; set; }

        /// <summary>
        /// Gets the timestamp (from <see cref="Stopwatch.GetTimestamp()"/>) captured
        /// immediately when this method invocation was recorded (effectively, the start time).
        /// </summary>
        /// <remarks>This property is set automatically during object construction.</remarks>
        internal long InvocationTick  { get; private set; }

        /// <summary>
        /// Gets or sets the timestamp (from <see cref="Stopwatch.GetTimestamp()"/>) captured
        /// when the method invocation completed (effectively, the end time).
        /// This will be <c>null</c> if the method has not yet completed.
        /// </summary>
        internal long? ReturnTick { get; set; }

        internal ProfiledMethodInvocation()
        {
            InvocationTick = Stopwatch.GetTimestamp();
            MethodName = string.Empty;
        }
    }
}
