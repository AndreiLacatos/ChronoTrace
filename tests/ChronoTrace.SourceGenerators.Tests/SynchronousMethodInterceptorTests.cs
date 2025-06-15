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

    [Fact]
    public async Task SyncMethodWithParametersSingularInvocation_ShouldGenerateSingularSyncInterceptor()
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public void Do(int p1, string p2, double p3)
                {
                }
            }

            var subject = new S();
            subject.Do(default, default, default);
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task MultipleSyncMethodsSingularInvocation_ShouldGenerateMultipleSingularSyncInterceptors()
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public void Do()
                {
                }

                [ChronoTrace.Attributes.Profile]
                public void DoSomethingElse()
                {
                }
            }

            var subject = new S();
            subject.Do();
            subject.DoSomethingElse();
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task MultipleSyncMethodsMultipleInvocation_ShouldGenerateMultipleSyncInterceptors()
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public void Do()
                {
                }

                [ChronoTrace.Attributes.Profile]
                public void DoSomethingElse()
                {
                }
            }

            var subject = new S();
            subject.Do();
            subject.DoSomethingElse();
            subject.DoSomethingElse();
            subject.Do();
            subject.Do();
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
