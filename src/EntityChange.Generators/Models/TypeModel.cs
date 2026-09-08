using System;
using System.Collections.Generic;

namespace EntityChange.Generators.Models;

internal sealed class TypeModel : IEquatable<TypeModel>
{
    public string FullName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public List<PropertyModel> Properties { get; set; } = new();

    public bool Equals(TypeModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (FullName != other.FullName) return false;
        if (Properties.Count != other.Properties.Count) return false;
        for (int i = 0; i < Properties.Count; i++)
        {
            if (!Properties[i].Equals(other.Properties[i]))
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as TypeModel);
    public override int GetHashCode() => FullName.GetHashCode();
}
