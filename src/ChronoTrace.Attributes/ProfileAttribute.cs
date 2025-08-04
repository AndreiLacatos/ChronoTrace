namespace ChronoTrace.Attributes;

/// <summary>
/// Marks a method to be included in performance tracing by the <c>ChronoTrace</c> library.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ProfileAttribute : Attribute
{
    /// <summary>
    /// Controls tracing of adjacent method calls. When set to <c>true</c>, the library
    /// discovers methods called by the <c>[Profile]</c> attributed method and traces them.
    /// Opt-in feature, disabled by default.
    /// </summary>
    public bool Recursive { get; init; } = false;
}
