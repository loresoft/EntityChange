using System.Linq;

using EntityChange.Generators.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityChange.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class EntityChangeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "EntityChange.GenerateComparerAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (context, ct) => ExtractComparerInfo(context))
            .Where(static info => info != null);

        context.RegisterSourceOutput(pipeline, static (ctx, info) =>
        {
            if (info == null)
                return;

            var source = CodeEmitter.Generate(info);
            ctx.AddSource($"{info.ClassName}.g.cs", source);
        });
    }

    private static ComparerInfo? ExtractComparerInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
            return null;

        // Must be partial
        var syntax = context.TargetNode as ClassDeclarationSyntax;
        if (syntax == null)
            return null;

        var isPartial = syntax.Modifiers.Any(m => m.ValueText == "partial");
        if (!isPartial)
            return null;

        // Must derive from EntityComparer<T>
        var baseType = classSymbol.BaseType;
        if (baseType == null || !baseType.IsGenericType)
            return null;

        var baseTypeName = baseType.OriginalDefinition.ToDisplayString();
        if (baseTypeName != "EntityChange.EntityComparer<T>")
            return null;

        var targetType = baseType.TypeArguments[0] as INamedTypeSymbol;
        if (targetType == null)
            return null;

        var accessibility = classSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.Private => "private",
            _ => "public"
        };

        var info = new ComparerInfo
        {
            ClassName = classSymbol.Name,
            Namespace = classSymbol.ContainingNamespace?.IsGlobalNamespace == true
                ? ""
                : classSymbol.ContainingNamespace?.ToDisplayString() ?? "",
            Accessibility = accessibility,
            TargetTypeFullName = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            TargetTypeShortName = targetType.Name,
            TypeModels = TypeAnalyzer.BuildTypeModels(targetType),
        };

        return info;
    }
}
