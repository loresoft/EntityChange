using System;

namespace EntityChange.Generators.Models;

internal enum PropertyCategory
{
    Value,
    Complex,
    Collection,
    Set,
    GenericDictionary,
    NonGenericDictionary,
    Array,
    Polymorphic,
}

internal sealed class PropertyModel : IEquatable<PropertyModel>
{
    public string Name { get; set; } = string.Empty;
    public string TypeFullName { get; set; } = string.Empty;
    public string TypeShortName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? FormatString { get; set; }
    public bool IsIgnored { get; set; }
    public bool IsNullable { get; set; }
    public PropertyCategory Category { get; set; }

    // For collections/arrays: element type
    public string? ElementTypeFullName { get; set; }
    public string? ElementTypeShortName { get; set; }
    public bool ElementIsComplex { get; set; }

    // For dictionaries: key + value types
    public string? DictionaryKeyTypeFullName { get; set; }
    public string? DictionaryValueTypeFullName { get; set; }
    public bool DictionaryValueIsComplex { get; set; }

    public bool Equals(PropertyModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name
            && TypeFullName == other.TypeFullName
            && TypeShortName == other.TypeShortName
            && DisplayName == other.DisplayName
            && FormatString == other.FormatString
            && IsIgnored == other.IsIgnored
            && IsNullable == other.IsNullable
            && Category == other.Category
            && ElementTypeFullName == other.ElementTypeFullName
            && ElementTypeShortName == other.ElementTypeShortName
            && ElementIsComplex == other.ElementIsComplex
            && DictionaryKeyTypeFullName == other.DictionaryKeyTypeFullName
            && DictionaryValueTypeFullName == other.DictionaryValueTypeFullName
            && DictionaryValueIsComplex == other.DictionaryValueIsComplex;
    }

    public override bool Equals(object? obj) => Equals(obj as PropertyModel);
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Name.GetHashCode();
            hash = hash * 31 + TypeFullName.GetHashCode();
            hash = hash * 31 + (int)Category;
            return hash;
        }
    }
}
