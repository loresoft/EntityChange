namespace EntityChange;

/// <summary>
/// Excludes a property from comparison when used with source-generated comparers.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CompareIgnoreAttribute : Attribute;
