using System;
using System.Collections.Generic;

namespace EntityChange.Generators.Models;

internal sealed class ComparerInfo : IEquatable<ComparerInfo>
{
    public string ClassName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Accessibility { get; set; } = "public";
    public string TargetTypeFullName { get; set; } = string.Empty;
    public string TargetTypeShortName { get; set; } = string.Empty;
    public List<TypeModel> TypeModels { get; set; } = new();

    public bool Equals(ComparerInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (ClassName != other.ClassName) return false;
        if (Namespace != other.Namespace) return false;
        if (Accessibility != other.Accessibility) return false;
        if (TargetTypeFullName != other.TargetTypeFullName) return false;
        if (TypeModels.Count != other.TypeModels.Count) return false;
        for (int i = 0; i < TypeModels.Count; i++)
        {
            if (!TypeModels[i].Equals(other.TypeModels[i]))
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as ComparerInfo);
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + ClassName.GetHashCode();
            hash = hash * 31 + Namespace.GetHashCode();
            hash = hash * 31 + TargetTypeFullName.GetHashCode();
            return hash;
        }
    }
}
