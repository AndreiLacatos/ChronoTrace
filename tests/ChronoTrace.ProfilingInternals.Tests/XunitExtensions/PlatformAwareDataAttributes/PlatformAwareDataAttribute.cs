using System.Runtime.InteropServices;
using Xunit.Sdk;

namespace ChronoTrace.ProfilingInternals.Tests.XunitExtensions.PlatformAwareDataAttributes;

internal abstract class PlatformAwareDataAttribute : DataAttribute
{
    private readonly OSPlatform _platform;
    private readonly object[] _data;

    protected PlatformAwareDataAttribute(OSPlatform platform, params object[] data)
    {
        _platform = platform;
        _data = data;
    }

    public override IEnumerable<object[]> GetData(System.Reflection.MethodInfo testMethod)
    {
        if (RuntimeInformation.IsOSPlatform(_platform))
        {
            yield return _data;
        }
        else
        {
            Assert.True(true);
        }
    }
}
