using System.Runtime.InteropServices;

namespace ChronoTrace.ProfilingInternals.Tests.XunitExtensions.PlatformAwareDataAttributes;

internal sealed class WindowsDataAttribute : PlatformAwareDataAttribute
{
    public WindowsDataAttribute(params object[] data) : base(OSPlatform.Windows, data)
    {
    }
}
