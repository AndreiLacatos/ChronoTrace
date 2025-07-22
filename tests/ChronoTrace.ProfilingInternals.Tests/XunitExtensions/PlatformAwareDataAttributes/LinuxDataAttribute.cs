using System.Runtime.InteropServices;

namespace ChronoTrace.ProfilingInternals.Tests.XunitExtensions.PlatformAwareDataAttributes;

internal sealed class LinuxDataAttribute : PlatformAwareDataAttribute
{
    public LinuxDataAttribute(params object[] data) : base(OSPlatform.Linux, data)
    {
    }
}
