using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EntityChange.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EntityChangeAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "EC0001",
        title: "Class with [GenerateComparer] must be partial",
        messageFormat: "Class '{0}' has [GenerateComparer] but is not declared as partial",
        category: "EntityChange",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MustDeriveFromBase = new(
        id: "EC0002",
        title: "Class with [GenerateComparer] must derive from EntityComparer<T>",
        messageFormat: "Class '{0}' has [GenerateComparer] but does not derive from EntityComparer<T>",
        category: "EntityChange",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TargetTypeMustBeAccessible = new(
        id: "EC0003",
        title: "EntityComparer<T> type parameter must be accessible",
        messageFormat: "Type parameter '{0}' of EntityComparer<T> must be a reference type with accessible properties",
        category: "EntityChange",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DisplayFormatOnNonFormattable = new(
        id: "EC0004",
        title: "[DisplayFormat] on non-formattable type",
        messageFormat: "Property '{0}' has [DisplayFormat] but its type '{1}' may not support formatting",
        category: "EntityChange",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PolymorphicPropertyInfo = new(
        id: "EC0005",
        title: "Polymorphic property will use Equals comparison",
        messageFormat: "Property '{0}' is abstract/interface type '{1}' — will be compared using Equals() instead of deep comparison",
        category: "EntityChange",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            MustBePartial,
            MustDeriveFromBase,
            TargetTypeMustBeAccessible,
            DisplayFormatOnNonFormattable,
            PolymorphicPropertyInfo);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol classSymbol)
            return;

        // Check for [GenerateComparer] attribute
        var hasAttribute = classSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == "EntityChange.GenerateComparerAttribute");

        if (!hasAttribute)
            return;

        // EC0001: Must be partial
        var syntaxRefs = classSymbol.DeclaringSyntaxReferences;
        var isPartial = syntaxRefs.Any(r =>
        {
            var node = r.GetSyntax(context.CancellationToken);
            return node is ClassDeclarationSyntax cds
                && cds.Modifiers.Any(SyntaxKind.PartialKeyword);
        });

        if (!isPartial)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MustBePartial, classSymbol.Locations[0], classSymbol.Name));
        }

        // EC0002: Must derive from EntityComparer<T>
        var baseType = classSymbol.BaseType;
        var derivesFromBase = baseType != null
            && baseType.IsGenericType
            && baseType.OriginalDefinition.ToDisplayString() == "EntityChange.EntityComparer<T>";

        if (!derivesFromBase)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MustDeriveFromBase, classSymbol.Locations[0], classSymbol.Name));
            return;
        }

        var targetType = baseType!.TypeArguments[0];

        // EC0003: Target type must be a reference type with accessible properties
        if (targetType.IsValueType || targetType.SpecialType == SpecialType.System_String)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                TargetTypeMustBeAccessible, classSymbol.Locations[0], targetType.Name));
        }

        // Check properties on the target type for EC0004 and EC0005
        if (targetType is INamedTypeSymbol namedTarget)
        {
            AnalyzeTargetProperties(context, namedTarget);
        }
    }

    private static void AnalyzeTargetProperties(SymbolAnalysisContext context, INamedTypeSymbol targetType)
    {
        var current = targetType;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.IsStatic || member.IsIndexer || member.GetMethod == null)
                    continue;
                if (member.DeclaredAccessibility != Accessibility.Public)
                    continue;

                AnalyzeProperty(context, member);
            }

            current = current.BaseType;
        }
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context, IPropertySymbol property)
    {
        var type = property.Type;

        // Unwrap nullable
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            type = nullable.TypeArguments[0];

        // EC0005: Polymorphic property info
        if (!type.IsValueType && type.SpecialType != SpecialType.System_String
            && (type.IsAbstract || type.TypeKind == TypeKind.Interface))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                PolymorphicPropertyInfo, property.Locations[0], property.Name, type.Name));
        }

        // EC0004: [DisplayFormat] on non-formattable type
        var hasDisplayFormat = property.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.DisplayFormatAttribute");

        if (hasDisplayFormat)
        {
            // Check if type implements IFormattable
            var isFormattable = type.AllInterfaces.Any(i => i.ToDisplayString() == "System.IFormattable")
                || type.IsValueType
                || type.SpecialType == SpecialType.System_String;

            if (!isFormattable)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DisplayFormatOnNonFormattable, property.Locations[0], property.Name, type.Name));
            }
        }
    }
}
