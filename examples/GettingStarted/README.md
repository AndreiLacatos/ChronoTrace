# Getting started with ChronoTrace

Just annotate your desired methods with the `[Profile]` attribute. That's right, no other code changes are needed!

```csharp
public class Demo
{
    [Profile] // ⬅️ That's it!
    public async Task DoWorkAsync()
    {
        // ...
    }
}
```

Next, a tiny addition to your project's `.csproj` file to enable the magic:

```xml
    <PropertyGroup>
        <!-- ⬇️ Pop this line in to activate interceptors ⬇️  -->
        <InterceptorsNamespaces>$(InterceptorsNamespaces);ProfilingInterceptors</InterceptorsNamespaces>
    </PropertyGroup>
```

🔨 All set! Rebuild, run & enjoy!

## Customize trace output location

By default, `ChronoTrace` tucks your performance reports away in `./timings/report.json.` Want to stash them somewhere else or give the file a  new name?

Simply add the `<ChronoTraceTimingOutput>` property to your `.csproj` and point it to your preferred spot:

```xml
    <PropertyGroup>
        <!-- ⬇️ Full or relative paths, custom filenames – you call the shots! ⬇️  -->
        <ChronoTraceTimingOutput>$(SolutionDir)demo\report_demo.json</ChronoTraceTimingOutput>
    </PropertyGroup>
```

One more tiny step to make sure the compiler is aware of it:
```xml
    <ItemGroup>
        <CompilerVisibleProperty Include="SolutionDir" /> <!--  ⬅️ Only required if you specify $(SolutionDir) in the path -->
        <CompilerVisibleProperty Include="ChronoTraceTimingOutput" />
    </ItemGroup>
```

To write traces to `stdout` instead of a file, set the output path to stdout.
```xml
    <PropertyGroup>
        <!-- ⬇️ That does the trick! ⬇️  -->
        <ChronoTraceTimingOutput>stdout</ChronoTraceTimingOutput>
    </PropertyGroup>
```

Your traces will now land exactly where you want them. 🎯