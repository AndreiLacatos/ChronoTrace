namespace ChronoTrace.SourceGenerators.Tests;

public class AsynchronousMethodInterceptorTests
{
    [Fact]
    public async Task AsyncMethodSingularInvocation_ShouldGenerateSingularAsyncInterceptor()
    {
        var source = 
            """
            using System.Threading.Tasks;

            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public async Task Do()
                {
                }
            }

            var subject = new S();
            await subject.Do();
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task AsyncValueTaskMethodSingularInvocation_ShouldGenerateSingularAsyncValueTaskInterceptor()
    {
        var source = 
            """
            using System.Threading.Tasks;

            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public async ValueTask Do()
                {
                }
            }

            var subject = new S();
            await subject.Do();
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task AsyncScalarMethodSingularInvocation_ShouldGenerateSingularAsyncInterceptor()
    {
        var source = 
            """
            using System.Threading.Tasks;

            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public async Task<int> Do()
                {
                    return 0;
                }
            }

            var subject = new S();
            await subject.Do();
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task AsyncValueTaskScalarMethodSingularInvocation_ShouldGenerateSingularAsyncInterceptor()
    {
        var source = 
            """
            using System.Threading.Tasks;

            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public async ValueTask<int> Do()
                {
                    return 0;
                }
            }

            var subject = new S();
            await subject.Do();
            """;

        var driver = SourceGenerationRunner.Run(source);
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
