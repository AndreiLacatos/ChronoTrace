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

    [Fact]
    public async Task SyncMethodCallingExternalMethods_ShouldGenerateInterceptorForEachMethod()
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile(Recursive = true)]
                public void Do()
                {
                    DoSomethingElse();
                }

                public void DoSomethingElse()
                {
                    var t = new T();
                    t.DoSomeOtherThing();
                }
            }

            public class T
            {
                public void DoSomeOtherThing()
                {
                    System.Console.WriteLine("Working...");
                }
            }

            var subject = new S();
            subject.Do();
            """;

        var (driver, _) = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task RecursiveSyncMethod_ShouldGenerateInterceptorForEachMethod()
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile(Recursive = true)]
                public void Driver()
                {
                    RecursiveMethod();
                }

                public void RecursiveMethod()
                {
                    SomeHelper();
                    RecursiveMethod();
                }

                public void SomeHelper()
                {
                    Console.WriteLine("Working...");
                }
            }

            var subject = new S();
            subject.Driver();
            """;

        var (driver, _) = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task IndirectRecursiveSyncMethod_ShouldGenerateInterceptorForEachMethod()
    {
        var source = 
            """
            public class S
            {
                [ChronoTrace.Attributes.Profile(Recursive = true)]
                public void Driver()
                {
                    BranchAlpha();
                }

                public void BranchAlpha()
                {
                    AlphaHelper();
                    BranchBravo();
                }
                
                public void BranchBravo()
                {
                    BravoHelper();
                    BranchAlpha();
                    CharlieHelper();
                }

                public void AlphaHelper()
                {
                    Console.WriteLine("Working...");
                }

                public void BravoHelper()
                {
                    Console.WriteLine("Working...");
                }

                public void CharlieHelper()
                {
                    Console.WriteLine("Working...");
                }
            }

            var subject = new S();
            subject.Driver();
            """;

        var (driver, _) = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());
        await Verify(driver).UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
