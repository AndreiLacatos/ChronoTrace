using ChronoTrace.Attributes;

namespace GettingStarted;

public class Demo
{
    [Profile]
    public async Task DoWorkAsync()
    {
        Console.WriteLine("Working...");
        await Task.Delay(new Random().Next(100, 400));
        Console.WriteLine("Done!");
    }
}
