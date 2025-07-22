# ChronoTrace

![Build & Test](https://github.com/AndreiLacatos/ChronoTrace/actions/workflows/tests.yml/badge.svg?branch=master)

A package for simple, function-level performance tracing.

Ever found yourself hunting down performance bottlenecks? Manually wrapping methods with `Stopwatch` code to pinpoint slow sections is tedious and error-prone.

```csharp
    // ❌ Say goodbye to this:
    var sw = Stopwatch.StartNew();
    var worker = new Worker();
    await worker.DoWork();
    Console.WriteLine($"DoWork took: {sw.Elapsed}");
```

## Installing the package

Easily add ChronoTrace to your project:

```sh
dotnet add package ChronoTrace
```

## Using the package

Just decorate the methods you want to trace with the `[Profile]` attribute, the package handles the rest.

```csharp
public class Worker
{
    [Profile] // ⬅️ That's it!
    public async Task DoWork()
    {
        // ...
    }
}
```
