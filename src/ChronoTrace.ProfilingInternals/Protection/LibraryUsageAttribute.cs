using System;

namespace ChronoTrace.ProfilingInternals.Protection
{
    /// <summary>
    /// Marks a public API as intended for internal use only. A compile-time warning is issued on any external usage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method
        | AttributeTargets.Property | AttributeTargets.Constructor)]
    internal sealed class LibraryUsageAttribute : Attribute
    {
    }
}
