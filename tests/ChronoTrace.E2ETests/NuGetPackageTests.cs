namespace ChronoTrace.E2ETests;

public class NuGetPackageTests
{
    public class Subject
    {
        [ChronoTrace.Attributes.Profile]
        public void Work()
        {
            System.Console.WriteLine("Working...");
        }
    }  

    [Fact]
    public void NugetPackageSmokeTest_ShouldGenerateTimingOutput()
    {
        var sut = new Subject();
        sut.Work();

        var outputDirectoryPath = GetPath("timings");
        var searchPattern = "report*.json";

        // assert that the directory was created.
        Assert.True(Directory.Exists(outputDirectoryPath));

        // assert that a file matching the pattern exists within that directory
        var filesFound = Directory.EnumerateFiles(outputDirectoryPath, searchPattern);
        Assert.Single(filesFound);
    }

    private string GetPath(
        string relativePath,
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
    {
        var sourceDirectory = Path.GetDirectoryName(sourceFilePath);
        var fullPath = Path.Combine(sourceDirectory!, relativePath);
        return Path.GetFullPath(fullPath);
    }
}
