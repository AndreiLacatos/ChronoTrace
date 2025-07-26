# .NETStandard 2.1 Migration Notes

This branch attempts to migrate the package to `netstandard2.1`.

## Outstanding Issues
* Lack of C# 9+ features in `netstandard2.1`:
    * `required` keyword is unsupported; workarounds needed
    * `init`-only properties are unavailable; replaced with mutable properties
    * `record` types replaced with `class`; these require further validation to ensure semantics aren't broken

`System.Text.Json` is no longer included as a BCL reference under `netstandard2.1`; added as a NuGet dependency instead.
 
## Build & Packaging Problems

When packaging the project, the resulting .nupkg does not include System.Text.Json.dll. This causes runtime failure in the source generator, manifesting as a misleading warning:

```sh
CSC : warning CS8784: Generator 'InterceptorGenerator' failed to initialize. It will not contribute to the output and compilation errors may occur as a result. Exception was of type 'FileNotFoundException' with message 'Could not load file or assembly 'ChronoTrace.ProfilingInternals, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'.
```

What’s actually happening:

1. `ChronoTrace.ProfilingInternals.dll` is loaded by the generator
2. It depends on `System.Text.Json.dll`, but that dependency is missing from the NuGet package
3. The load failure is reported as if ProfilingInternals.dll itself is missing

### Attempted Workarounds

Adding the following to either the generator's .csproj or the internals library .csproj (or both) had no effect on packaging:
```xml
<PropertyGroup>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
</PropertyGroup>
```
