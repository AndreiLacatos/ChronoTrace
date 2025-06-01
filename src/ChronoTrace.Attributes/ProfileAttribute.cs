namespace ChronoTrace.Attributes;

/// <summary>
/// Marks a method to be included in performance tracing by the <c>ChronoTrace</c> library.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ProfileAttribute : Attribute;