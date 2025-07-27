using System;

namespace ChronoTrace.SourceGenerators.Compat
{
    internal class BuildTimeProvider : IBuildTimeProvider
    {
        public DateTimeOffset GetUtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
