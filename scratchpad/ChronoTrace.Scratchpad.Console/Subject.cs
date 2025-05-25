using ChronoTrace.Attributes;

namespace ChronoTrace.Scratchpad.Console;

public class Subject
{
    [Profile]
    public void PerformAction()
    {
        System.Console.WriteLine("Working...");
    }
}
