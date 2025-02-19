using EntityChange.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityChange.Generator;

[Generator]
public class ChangeDetectorGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat _fullyQualifiedNullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "EntityChange.Attributes.ChangeDetectorAttribute",
            predicate: SyntacticPredicate,
            transform: SemanticTransform
        )
        .Where(static context => context is not null);

        // Emit the diagnostics, if needed
        var diagnostics = provider
            .Select(static (item, _) => item?.Diagnostics)
            .Where(static item => item?.Count > 0);

        context.RegisterSourceOutput(diagnostics, ReportDiagnostic);

        // output code
        var entityClasses = provider
            .Select(static (item, _) => item?.ChangeDetector)
            .Where(static item => item is not null);

        context.RegisterSourceOutput(entityClasses, Execute);
    }

    private static void ReportDiagnostic(SourceProductionContext context, EquatableArray<Diagnostic>? diagnostics)
    {
        if (diagnostics == null)
            return;

        foreach (var diagnostic in diagnostics)
            context.ReportDiagnostic(diagnostic);
    }

    private static void Execute(SourceProductionContext context, EntityChangeDetector? entityClass)
    {
        if (entityClass == null)
            return;

        var qualifiedName = entityClass.ClassNamespace is null
            ? entityClass.ClassName
            : $"{entityClass.ClassNamespace}.{entityClass.ClassName}";

        //var source = ChangeDetectorWriter.Generate(entityClass);

        //context.AddSource($"{qualifiedName}.ChangeDetector.g.cs", source);
    }


    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode is ClassDeclarationSyntax classDeclaration && !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
            || syntaxNode is RecordDeclarationSyntax recordDeclaration && !recordDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
            || syntaxNode is StructDeclarationSyntax structDeclaration && !structDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
    }

    private static EntityChangeContext? SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
            return null;

        var diagnostics = new List<Diagnostic>();

        var fullyQualified = targetSymbol.ToDisplayString(_fullyQualifiedNullableFormat);
        var classNamespace = targetSymbol.ContainingNamespace.ToDisplayString();
        var className = targetSymbol.Name;

        var methodSymbols = GetMethods(targetSymbol);

        var methodArray = methodSymbols
            .Select(symbol => CreateMethod(diagnostics, symbol))
            .Where(item => item != null)
            .Select(item => item!)
            .ToArray();

        if (methodArray.Length == 0)
            return new EntityChangeContext(null, diagnostics.ToArray());

        var changeDetector = new EntityChangeDetector(
            FullyQualified: fullyQualified,
            ClassNamespace: classNamespace,
            ClassName: className,
            Methods: methodArray
        );

        return new EntityChangeContext(changeDetector, diagnostics.ToArray());
    }

    private static IEnumerable<IMethodSymbol> GetMethods(INamedTypeSymbol targetSymbol, bool includeBaseProperties = true)
    {
        var methods = new Dictionary<string, IMethodSymbol>();

        var currentSymbol = targetSymbol;

        // get nested methods
        while (currentSymbol != null)
        {
            var methodSymbols = currentSymbol
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Method)
                .OfType<IMethodSymbol>()
                .Where(IsIncluded)
                .Where(p => !methods.ContainsKey(p.Name));

            foreach (var methodSymbol in methodSymbols)
                methods.Add(methodSymbol.Name, methodSymbol);

            if (!includeBaseProperties)
                break;

            currentSymbol = currentSymbol.BaseType;
        }

        return methods.Values;
    }

    private static EntityChangeMethod? CreateMethod(List<Diagnostic> diagnostics, IMethodSymbol methodSymbol)
    {
        if (methodSymbol.Parameters.Length != 2)
            return null;

        var returnType = methodSymbol.ReturnType.ToDisplayString(_fullyQualifiedNullableFormat);
        var methodName = methodSymbol.Name;

        var parameters = methodSymbol.Parameters
            .Select(p => new EntityMethodParameter(p.Type.ToDisplayString(_fullyQualifiedNullableFormat), p.Name))
            .ToArray();

        var entityType = methodSymbol.Parameters[0].Type;
        if (entityType is not INamedTypeSymbol targetSymbol)
            return null;

        var entityClass = CreateClass(diagnostics, targetSymbol);

        return new EntityChangeMethod(
            ReturnType: returnType,
            MethodName: methodName,
            Parameters: parameters,
            EntityClass: entityClass
        );
    }

    private static bool IsIncluded(IMethodSymbol methodSymbol)
    {
        return methodSymbol.DeclaredAccessibility == Accessibility.Public
            && methodSymbol.IsPartialDefinition
            && methodSymbol.Parameters.Length == 2
            && SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[0].Type, methodSymbol.Parameters[1].Type);
    }

    private static EntityClass CreateClass(List<Diagnostic> diagnostics, INamedTypeSymbol targetSymbol)
    {
        var fullyQualified = targetSymbol.ToDisplayString(_fullyQualifiedNullableFormat);
        var classNamespace = targetSymbol.ContainingNamespace.ToDisplayString();
        var className = targetSymbol.Name;

        var propertySymbols = GetProperties(targetSymbol);

        var propertyArray = propertySymbols
            .Select(symbol => CreateProperty(diagnostics, symbol))
            .ToArray() ?? [];

        return new EntityClass(
            FullyQualified: fullyQualified,
            ClassNamespace: classNamespace,
            ClassName: className,
            Properties: propertyArray
        );
    }

    private static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol targetSymbol, bool includeBaseProperties = true)
    {
        var properties = new Dictionary<string, IPropertySymbol>();

        var currentSymbol = targetSymbol;

        // get nested properties
        while (currentSymbol != null)
        {
            var propertySymbols = currentSymbol
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>()
                .Where(IsIncluded)
                .Where(p => !properties.ContainsKey(p.Name));

            foreach (var propertySymbol in propertySymbols)
                properties.Add(propertySymbol.Name, propertySymbol);

            if (!includeBaseProperties)
                break;

            currentSymbol = currentSymbol.BaseType;
        }

        return properties.Values;
    }

    private static EntityProperty CreateProperty(List<Diagnostic> diagnostics, IPropertySymbol propertySymbol)
    {
        var propertyType = propertySymbol.Type.ToDisplayString(_fullyQualifiedNullableFormat);
        var propertyName = propertySymbol.Name;
        var displayName = propertyName;
        bool ignore = false;
        string? stringFormat = null;

        // look for custom equality
        var attributes = propertySymbol.GetAttributes();
        var attribute = attributes.FirstOrDefault(IsKnownAttribute);

        if (attribute == null)
        {
            return new EntityProperty(
                PropertyName: propertyName,
                PropertyType: propertyType,
                Ignore: ignore,
                DisplayName: displayName,
                StringFormat: stringFormat);
        }

        foreach (var parameter in attribute.NamedArguments)
        {
            var name = parameter.Key;
            var value = parameter.Value.Value;

            if (string.IsNullOrEmpty(name) || value == null)
                continue;

            switch (name)
            {
                case "DisplayName":
                    displayName = value.ToString();
                    break;
                case "Ignore":
                    ignore = value is true;
                    break;
                case "StringFormat":
                    stringFormat = parameter.Value.ToCSharpString();
                    break;
            }
        }

        return new EntityProperty(
            PropertyName: propertyName,
            PropertyType: propertyType,
            Ignore: ignore,
            DisplayName: displayName,
            StringFormat: stringFormat);
    }

    private static bool IsIncluded(IPropertySymbol propertySymbol)
    {
        return !propertySymbol.IsIndexer && propertySymbol.DeclaredAccessibility == Accessibility.Public;
    }



    private static bool IsKnownAttribute(AttributeData attribute)
    {
        if (attribute == null)
            return false;

        return attribute.AttributeClass is
        {
            Name: "ChangePropertyAttribute",
            ContainingNamespace:
            {
                Name: "Attributes",
                ContainingNamespace.Name: "EntityChange"
            }
        };

    }


    private static bool IsValueType(INamedTypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: nameof(ValueType) or nameof(Object),
            ContainingNamespace.Name: "System"
        };
    }

    private static bool IsEnumerable(INamedTypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: "IEnumerable",
            IsGenericType: true,
            TypeArguments.Length: 1,
            TypeParameters.Length: 1,
            ContainingNamespace:
            {
                Name: "Generic",
                ContainingNamespace:
                {
                    Name: "Collections",
                    ContainingNamespace:
                    {
                        Name: "System"
                    }
                }
            }
        };
    }

    private static bool IsDictionary(INamedTypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: "IDictionary",
            IsGenericType: true,
            TypeArguments.Length: 2,
            TypeParameters.Length: 2,
            ContainingNamespace:
            {
                Name: "Generic",
                ContainingNamespace:
                {
                    Name: "Collections",
                    ContainingNamespace:
                    {
                        Name: "System"
                    }
                }
            }
        };
    }


    private static bool IsString(ITypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: nameof(String),
            ContainingNamespace.Name: "System"
        };
    }

}
