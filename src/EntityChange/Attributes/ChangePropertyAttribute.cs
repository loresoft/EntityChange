using System.Diagnostics;

namespace EntityChange.Attributes;

[Conditional("ENTITYCHANGE_GENERATOR")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ChangePropertyAttribute(string propertyName) : Attribute
{
    public string PropertyName { get; } = propertyName;

    public string? DisplayName { get; set; }


    public bool Ignore { get; set; }


    public string? StringFormat { get; set; }

    public string? FormatProvider { get; set; }


    public Type? EqualityType { get; set; }

    public string? EqualityInstance { get; set; }
}


[Conditional("ENTITYCHANGE_GENERATOR")]
[AttributeUsage(AttributeTargets.Class)]
public class ChangeDetectorAttribute : Attribute;
