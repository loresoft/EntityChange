using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using EntityChange.Generators.Models;

using Microsoft.CodeAnalysis;

namespace EntityChange.Generators;

internal static class TypeAnalyzer
{
    private static readonly Regex WordsExpression = new("([A-Z][a-z]*)|([0-9]+)");

    public static List<TypeModel> BuildTypeModels(INamedTypeSymbol targetType)
    {
        var models = new List<TypeModel>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        WalkType(targetType, models, visited);
        return models;
    }

    private static void WalkType(INamedTypeSymbol typeSymbol, List<TypeModel> models, HashSet<string> visited)
    {
        var fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (!visited.Add(fullName))
            return;

        var typeModel = new TypeModel
        {
            FullName = fullName,
            ShortName = typeSymbol.Name,
            Properties = new List<PropertyModel>(),
        };

        // Walk the inheritance chain: base types first
        var hierarchy = GetTypeHierarchy(typeSymbol);

        foreach (var type in hierarchy)
        {
            var members = type.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic
                    && !p.IsIndexer
                    && p.GetMethod != null
                    && p.DeclaredAccessibility == Accessibility.Public);

            foreach (var property in members)
            {
                var model = AnalyzeProperty(property);
                if (!model.IsIgnored)
                    typeModel.Properties.Add(model);
            }
        }

        models.Add(typeModel);

        // Recursively walk nested complex types
        foreach (var prop in typeModel.Properties)
        {
            if (prop.Category == PropertyCategory.Complex)
            {
                var nestedType = FindNamedType(prop.TypeFullName, typeSymbol);
                if (nestedType != null)
                    WalkType(nestedType, models, visited);
            }

            if (prop.ElementIsComplex && prop.ElementTypeFullName != null)
            {
                var elementType = FindNamedTypeFromCompilation(prop.ElementTypeFullName, typeSymbol);
                if (elementType != null)
                    WalkType(elementType, models, visited);
            }

            if (prop.DictionaryValueIsComplex && prop.DictionaryValueTypeFullName != null)
            {
                var valueType = FindNamedTypeFromCompilation(prop.DictionaryValueTypeFullName, typeSymbol);
                if (valueType != null)
                    WalkType(valueType, models, visited);
            }
        }
    }

    private static List<INamedTypeSymbol> GetTypeHierarchy(INamedTypeSymbol typeSymbol)
    {
        var hierarchy = new List<INamedTypeSymbol>();
        var current = typeSymbol;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            hierarchy.Add(current);
            current = current.BaseType;
        }

        hierarchy.Reverse(); // base first
        return hierarchy;
    }

    private static PropertyModel AnalyzeProperty(IPropertySymbol property)
    {
        var unwrappedType = property.Type;
        if (unwrappedType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nts)
            unwrappedType = nts.TypeArguments[0];

        var model = new PropertyModel
        {
            Name = property.Name,
            TypeFullName = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            TypeShortName = unwrappedType.Name,
            DisplayName = GetDisplayName(property),
            FormatString = GetFormatString(property),
            IsIgnored = IsIgnored(property),
            IsNullable = property.Type.NullableAnnotation == NullableAnnotation.Annotated
                || property.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T,
        };

        CategorizeProperty(property.Type, model);
        return model;
    }

    private static void CategorizeProperty(ITypeSymbol typeSymbol, PropertyModel model)
    {
        // Unwrap nullable
        var type = typeSymbol;
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            type = nullable.TypeArguments[0];
            model.IsNullable = true;
        }

        // String is a value type for comparison purposes
        if (type.SpecialType == SpecialType.System_String)
        {
            model.Category = PropertyCategory.Value;
            return;
        }

        // Arrays
        if (type is IArrayTypeSymbol arrayType)
        {
            model.Category = PropertyCategory.Array;
            model.ElementTypeFullName = arrayType.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            model.ElementTypeShortName = arrayType.ElementType.Name;
            model.ElementIsComplex = IsComplexType(arrayType.ElementType);
            return;
        }

        // Check for ISet<T> / HashSet<T> / IReadOnlySet<T> before ICollection
        if (TryGetSetElementType(type, out var setElementType))
        {
            model.Category = PropertyCategory.Set;
            model.ElementTypeFullName = setElementType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            model.ElementTypeShortName = setElementType!.Name;
            model.ElementIsComplex = IsComplexType(setElementType!);
            return;
        }

        // IDictionary<TKey, TValue>
        if (TryGetGenericDictionaryTypes(type, out var keyType, out var valueType))
        {
            model.Category = PropertyCategory.GenericDictionary;
            model.DictionaryKeyTypeFullName = keyType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            model.DictionaryValueTypeFullName = valueType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            model.DictionaryValueIsComplex = IsComplexType(valueType!);
            return;
        }

        // Non-generic IDictionary
        if (ImplementsInterface(type, "System.Collections.IDictionary"))
        {
            model.Category = PropertyCategory.NonGenericDictionary;
            return;
        }

        // ICollection<T>, IList<T>, IEnumerable<T> (ordered collections)
        if (TryGetCollectionElementType(type, out var collElementType))
        {
            model.Category = PropertyCategory.Collection;
            model.ElementTypeFullName = collElementType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            model.ElementTypeShortName = collElementType!.Name;
            model.ElementIsComplex = IsComplexType(collElementType!);
            return;
        }

        // Value types and enums
        if (type.IsValueType)
        {
            model.Category = PropertyCategory.Value;
            return;
        }

        // Abstract / interface types → polymorphic fallback
        if (type.IsAbstract || type.TypeKind == TypeKind.Interface)
        {
            model.Category = PropertyCategory.Polymorphic;
            return;
        }

        // Sealed or concrete reference types with properties → complex
        if (type is INamedTypeSymbol namedType && namedType.TypeKind == TypeKind.Class)
        {
            var hasProperties = GetAllReadableProperties(namedType).Any();
            if (hasProperties)
            {
                model.Category = PropertyCategory.Complex;
                return;
            }
        }

        // Fallback: treat as value
        model.Category = PropertyCategory.Value;
    }

    private static bool IsComplexType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_String)
            return false;
        if (type.IsValueType)
            return false;
        if (type.IsAbstract || type.TypeKind == TypeKind.Interface)
            return false;
        if (type.SpecialType == SpecialType.System_Object)
            return false;
        if (type is INamedTypeSymbol named && named.TypeKind == TypeKind.Class)
            return GetAllReadableProperties(named).Any();
        return false;
    }

    private static IEnumerable<IPropertySymbol> GetAllReadableProperties(INamedTypeSymbol type)
    {
        var current = type;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (!member.IsStatic && !member.IsIndexer && member.GetMethod != null
                    && member.DeclaredAccessibility == Accessibility.Public)
                {
                    yield return member;
                }
            }
            current = current.BaseType;
        }
    }

    private static bool TryGetSetElementType(ITypeSymbol type, out ITypeSymbol? elementType)
    {
        elementType = null;

        // Check if type itself is ISet<T>, HashSet<T>, IReadOnlySet<T>
        if (type is INamedTypeSymbol named)
        {
            var iface = FindInterface(named, "System.Collections.Generic.ISet`1")
                     ?? FindInterface(named, "System.Collections.Generic.IReadOnlySet`1");
            if (iface != null)
            {
                elementType = iface.TypeArguments[0];
                return true;
            }
        }

        return false;
    }

    private static bool TryGetGenericDictionaryTypes(ITypeSymbol type, out ITypeSymbol? keyType, out ITypeSymbol? valueType)
    {
        keyType = null;
        valueType = null;

        if (type is INamedTypeSymbol named)
        {
            var iface = FindInterface(named, "System.Collections.Generic.IDictionary`2");
            if (iface != null)
            {
                keyType = iface.TypeArguments[0];
                valueType = iface.TypeArguments[1];
                return true;
            }
        }

        return false;
    }

    private static bool TryGetCollectionElementType(ITypeSymbol type, out ITypeSymbol? elementType)
    {
        elementType = null;

        if (type is INamedTypeSymbol named)
        {
            // Check IList<T>, ICollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>
            var iface = FindInterface(named, "System.Collections.Generic.IList`1")
                     ?? FindInterface(named, "System.Collections.Generic.ICollection`1")
                     ?? FindInterface(named, "System.Collections.Generic.IReadOnlyList`1")
                     ?? FindInterface(named, "System.Collections.Generic.IReadOnlyCollection`1")
                     ?? FindInterface(named, "System.Collections.Generic.IEnumerable`1");
            if (iface != null)
            {
                elementType = iface.TypeArguments[0];
                return true;
            }
        }

        return false;
    }

    private static INamedTypeSymbol? FindInterface(INamedTypeSymbol type, string metadataName)
    {
        // Check if the type itself matches
        if (type.OriginalDefinition.ToDisplayString() == metadataName)
            return type;

        foreach (var iface in type.AllInterfaces)
        {
            if (iface.OriginalDefinition.ToDisplayString() == metadataName)
                return iface;
        }

        return null;
    }

    private static bool ImplementsInterface(ITypeSymbol type, string metadataName)
    {
        return type.AllInterfaces.Any(i => i.ToDisplayString() == metadataName);
    }

    private static string GetDisplayName(IPropertySymbol property)
    {
        // Priority: [Display(Name)] > [DisplayName] > PascalCase→Title
        foreach (var attr in property.GetAttributes())
        {
            var attrClass = attr.AttributeClass?.ToDisplayString();

            // [Display(Name = "...")]
            if (attrClass == "System.ComponentModel.DataAnnotations.DisplayAttribute")
            {
                foreach (var namedArg in attr.NamedArguments)
                {
                    if (namedArg.Key == "Name" && namedArg.Value.Value is string name)
                        return name;
                }
            }

            // [DisplayName("...")]
            if (attrClass == "System.ComponentModel.DisplayNameAttribute")
            {
                if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string displayName)
                    return displayName;
            }
        }

        return ToTitle(property.Name);
    }

    private static string? GetFormatString(IPropertySymbol property)
    {
        foreach (var attr in property.GetAttributes())
        {
            var attrClass = attr.AttributeClass?.ToDisplayString();

            if (attrClass == "System.ComponentModel.DataAnnotations.DisplayFormatAttribute")
            {
                foreach (var namedArg in attr.NamedArguments)
                {
                    if (namedArg.Key == "DataFormatString" && namedArg.Value.Value is string format)
                        return format;
                }
            }
        }

        return null;
    }

    private static bool IsIgnored(IPropertySymbol property)
    {
        foreach (var attr in property.GetAttributes())
        {
            var attrClass = attr.AttributeClass?.ToDisplayString();
            if (attrClass == "EntityChange.CompareIgnoreAttribute")
                return true;
            if (attrClass == "System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute")
                return true;
        }
        return false;
    }

    private static string ToTitle(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length < 2)
            return text;

        var words = WordsExpression.Matches(text);
        if (words.Count == 0)
            return text;

        var parts = new List<string>();
        foreach (Match word in words)
            parts.Add(word.Value);

        return string.Join(" ", parts);
    }

    private static INamedTypeSymbol? FindNamedType(string fullyQualifiedName, INamedTypeSymbol contextType)
    {
        return FindNamedTypeFromCompilation(fullyQualifiedName, contextType);
    }

    internal static INamedTypeSymbol? FindNamedTypeFromCompilation(string fullyQualifiedName, INamedTypeSymbol contextType)
    {
        // Strip the "global::" prefix if present
        var name = fullyQualifiedName;
        if (name.StartsWith("global::"))
            name = name.Substring("global::".Length);

        // Walk up containment to find compilation
        var compilation = contextType.ContainingAssembly;
        var result = compilation?.GetTypeByMetadataName(name);

        if (result != null)
            return result;

        // Check referenced assemblies
        if (contextType.ContainingModule?.ContainingSymbol is IAssemblySymbol assembly)
        {
            foreach (var module in assembly.Modules)
            {
                foreach (var referenced in module.ReferencedAssemblySymbols)
                {
                    result = referenced.GetTypeByMetadataName(name);
                    if (result != null)
                        return result;
                }
            }
        }

        return null;
    }
}
