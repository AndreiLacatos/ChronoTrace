namespace ChronoTrace.SourceGenerators.Tests;

public class SimpleInterceptorTests
{
    [Fact]
    public async Task SyncMethodSingularInvocation_ShouldGenerateSingularSyncInterceptor()
    {
        var source = 
            """
            public class Subject
            {
                [ChronoTrace.Attributes.Profile]
                public void PerformTask()
                {
                }
            }

            var subject = new Subject();
            subject.PerformTask();
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
