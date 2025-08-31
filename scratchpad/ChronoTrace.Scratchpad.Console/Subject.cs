using ChronoTrace.Attributes;

namespace ChronoTrace.Scratchpad.Console;

public class Subject
{
    [Profile]
    public void PerformAction()
    {
        System.Console.WriteLine("Working...");
        DoSomethingElse().GetAwaiter().GetResult();
        DoSomeOtherThing().GetAwaiter().GetResult();
    }

    public async Task DoSomethingElse()
    {
        System.Console.WriteLine("Doing something else...");
        await Task.Delay(TimeSpan.FromMilliseconds(400));
        await DoSomeOtherThing();
    }

    public async Task DoSomeOtherThing()
    {
        System.Console.WriteLine("Doing some other thing...");
        await Task.Delay(TimeSpan.FromMilliseconds(600));
    }
}


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

    [ChronoTrace.Attributes.Profile(Recursive = true)]
    public void Recursive(int i = 0)
    {
        if (i > 10)
        {
            return;
        }

        if (i % 2 == 0)
        {
            Hello();
        }
        
        System.Console.WriteLine(i);
        Recursive(i + 1);
    }

    public void Hello()
    {
        System.Console.WriteLine("Hello");
        Echo("Hi");
    }

    public void Echo(string s)
    {
        System.Console.WriteLine($"There you go: {s}");
    }
}

public class T
{
    public void DoSomeOtherThing()
    {
        System.Console.WriteLine("Working...");
    }
}