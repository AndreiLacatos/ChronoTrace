namespace ChronoTrace.SourceGenerators.Tests;

public class SynchronousMethodInterceptorTests
{
    [Fact]
    public async Task SyncMethodSingularInvocation_ShouldGenerateSingularSyncInterceptor()
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public void Do()
                {
                }
            }

            var subject = new S();
            subject.Do();
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task SyncScalarMethodSingularInvocation_ShouldGenerateSingularSyncInterceptor()
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public int Do()
                {
                    return 0;
                }
            }

            var subject = new S();
            subject.Do();
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
