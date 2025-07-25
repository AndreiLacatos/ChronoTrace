namespace ChronoTrace.SourceGenerators.DataStructures
{
    /// <summary>
    /// Specifies whether a method returns void or not in synchronous or asynchronous manner
    /// </summary>
    internal enum MethodType
    {
        /// <summary>
        /// Simple synchronous <c>void</c>
        /// </summary>
        SyncVoid = 0b10,

        /// <summary>
        /// Synchronous method, non-void return type
        /// </summary>
        SyncNonVoid = 0b00,

        /// <summary>
        /// Asynchronous method returning non-generic <c>Task</c>
        /// </summary>
        SimpleAsync = 0b11,

        /// <summary>
        /// Asynchronous method returning generic <c>Task&lt;&gt;</c>
        /// </summary>
        GenericAsync = 0b01,
    }
}
