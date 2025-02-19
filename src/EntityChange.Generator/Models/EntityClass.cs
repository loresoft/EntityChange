using Microsoft.CodeAnalysis;

namespace EntityChange.Generator.Models;

public record EntityChangeContext(
    EntityChangeDetector? ChangeDetector,
    EquatableArray<Diagnostic>? Diagnostics
);


public record EntityChangeDetector(
    string FullyQualified,
    string ClassNamespace,
    string ClassName,
    EquatableArray<EntityChangeMethod> Methods
);

public record EntityChangeMethod(
    string ReturnType,
    string MethodName,
    EquatableArray<EntityMethodParameter> Parameters,
    EntityClass EntityClass
);

public record EntityMethodParameter(
    string ParameterType,
    string ParameterName
);

public record EntityClass(
    string FullyQualified,
    string ClassNamespace,
    string ClassName,
    EquatableArray<EntityProperty> Properties
);


public record EntityProperty(
    string PropertyName,
    string PropertyType,
    bool Ignore,
    string? DisplayName,
    string? StringFormat
);
