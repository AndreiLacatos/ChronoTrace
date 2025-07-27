using System;

namespace ChronoTrace.SourceGenerators.Compat
{
    internal interface IBuildTimeProvider
    {
        DateTimeOffset GetUtcNow();
    }
}