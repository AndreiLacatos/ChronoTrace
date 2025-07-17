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

        var driver = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
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

        var driver = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
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

        var driver = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
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

        var driver = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task AsyncMethodWithParametersSingularInvocation_ShouldGenerateSingularAsyncInterceptor()
    {
        var source = 
            """
            using System.Threading.Tasks;

            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public async Task Do(int p1, string p2, double p3)
                {
                }
            }

            var subject = new S();
            await subject.Do(default, default, default);
            """;

        var driver = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task MultipleAsyncMethodsSingularInvocation_ShouldGenerateMultipleSingularAsyncInterceptors()
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

                [ChronoTrace.Attributes.Profile]
                public async Task DoSomethingElse()
                {
                }
            }

            var subject = new S();
            await subject.Do();
            await subject.DoSomethingElse();
            """;

        var driver = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task MultipleAsyncMethodsMultipleInvocation_ShouldGenerateMultipleAsyncInterceptors()
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

                [ChronoTrace.Attributes.Profile]
                public async Task DoSomethingElse()
                {
                }
            }

            var subject = new S();
            await subject.Do();
            await subject.DoSomethingElse();
            await subject.Do();
            await subject.DoSomethingElse();
            await subject.Do();
            """;

        var driver = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
