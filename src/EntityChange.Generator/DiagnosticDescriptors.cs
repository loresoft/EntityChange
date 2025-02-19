using Microsoft.CodeAnalysis;

namespace EntityChange.Generator;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor InvalidStringEqualityAttributeUsage => new(
        id: "EQ0010",
        title: "Invalid StringEquality Attribute Usage",
        messageFormat: "Invalid StringEquality attribute usage for property {0}.  Property type is not a string",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor InvalidDictionaryEqualityAttributeUsage => new(
        id: "EQ0011",
        title: "Invalid DictionaryEquality Attribute Usage",
        messageFormat: "Invalid DictionaryEquality attribute usage for property {0}.  Property type does not implement IDictionary<TKey, TValue>",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor InvalidHashSetEqualityAttributeUsage => new(
        id: "EQ0012",
        title: "Invalid HashSetEquality Attribute Usage",
        messageFormat: "Invalid HashSetEquality attribute usage for property {0}.  Property type does not implement IEnumerable<T>",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor InvalidSequenceEqualityAttributeUsage => new(
        id: "EQ0013",
        title: "Invalid SequenceEquality Attribute Usage",
        messageFormat: "Invalid SequenceEquality attribute usage for property {0}.  Property type does not implement IEnumerable<T>",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

}
