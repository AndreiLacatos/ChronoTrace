namespace ChronoTrace.SourceGenerators.Tests;

public partial class SynchronousMethodInterceptorTests
{
    [Fact]
    public async Task SyncMethodCallingOtherMethods_ShouldGenerateInterceptorForEachMethod()
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile]
                public void Do()
                {
                    DoSomethingElse();
                    DoSomeOtherThing();
                }

                [ChronoTrace.Attributes.Profile]
                public void DoSomethingElse()
                {
                    DoSomeOtherThing();
                }

                [ChronoTrace.Attributes.Profile]
                public void DoSomeOtherThing()
                {
                }
            }

            var subject = new S();
            subject.Do();
            """;

        var (driver, _) = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
