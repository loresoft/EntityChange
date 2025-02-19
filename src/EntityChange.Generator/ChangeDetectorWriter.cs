using EntityChange.Generator.Models;

using Microsoft.CodeAnalysis;

namespace EntityChange.Generator;

public static class ChangeDetectorWriter
{
    public static string Generate(EntityClass entityClass)
    {
        if (entityClass is null)
            throw new ArgumentNullException(nameof(entityClass));

        var codeBuilder = new IndentedStringBuilder();
        codeBuilder
            .AppendLine("// <auto-generated />")
            .AppendLine("#nullable enable")
            .AppendLine();

        codeBuilder
            .Append("namespace ")
            .AppendLine(entityClass.ClassNamespace)
            .AppendLine("{")
            .IncrementIndent();

        codeBuilder
            .Append("public partial class ")
            .Append(entityClass.ClassName)
            .Append("ChangeDetector")
            .Append(" : global::EntityChange.IChangeDetector<")
            .Append(entityClass.FullyQualified)
            .Append(">");


        codeBuilder
            .AppendLine()
            .AppendLine("{")
            .IncrementIndent();

        GenerateFields(codeBuilder);
        GenerateCompareMethod(codeBuilder, entityClass);

        foreach (var entityProperty in entityClass.Properties.Where(p => !p.Ignore))
        {
            GeneratePropertyCompary(codeBuilder, entityClass, entityProperty);
        }

        codeBuilder
            .DecrementIndent()
            .AppendLine("}"); // class

        codeBuilder
            .DecrementIndent()
            .AppendLine("}"); // namespace

        return codeBuilder.ToString();
    }

    private static void GenerateFields(IndentedStringBuilder codeBuilder)
    {
        codeBuilder
            .AppendLine("private readonly global::System.Collections.Generic.List<global::EntityChange.ChangeRecord> _changes = [];")
            .AppendLine();


    }

    private static void GenerateCompareMethod(IndentedStringBuilder codeBuilder, EntityClass entityClass)
    {
        codeBuilder
             .AppendLine("/// <inheritdoc />")
             .Append("[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"")
             .Append(ThisAssembly.Product)
             .Append("\", \"")
             .Append(ThisAssembly.InformationalVersion)
             .AppendLine("\")]")
             .Append("public virtual global::System.Collections.Generic.IReadOnlyList<global::EntityChange.ChangeRecord> DetectChanges(")
             .Append(entityClass.FullyQualified)
             .Append("?")
             .Append(" original, ")
             .Append(entityClass.FullyQualified)
             .Append("?")
             .Append(" current)")
             .AppendLine()
             .AppendLine("{")
             .IncrementIndent();

        codeBuilder
            .AppendLine("_changes.Clear();")
            .AppendLine();

        codeBuilder
            .AppendLine("if (original == null || current == null)")
            .AppendLine("    return _changes;");

        codeBuilder.AppendLine();

        foreach (var entityProperty in entityClass.Properties.Where(p => !p.Ignore))
        {
            codeBuilder
                 .Append("CompareProperty")
                 .Append(entityClass.ClassName)
                 .Append(entityProperty.PropertyName)
                 .AppendLine("(original, current);");
        }

        codeBuilder
            .AppendLine()
            .AppendLine("return _changes;");

        codeBuilder
            .DecrementIndent()
            .AppendLine("}")
            .AppendLine();
    }

    private static void GeneratePropertyCompary(IndentedStringBuilder codeBuilder, EntityClass entityClass, EntityProperty entityProperty)
    {
        codeBuilder
             .Append("private void CompareProperty")
             .Append(entityClass.ClassName)
             .Append(entityProperty.PropertyName)
             .Append("(")
             .Append(entityClass.FullyQualified)
             .Append(" original, ")
             .Append(entityClass.FullyQualified)
             .Append(" current)")
             .AppendLine()
             .AppendLine("{")
             .IncrementIndent();

        codeBuilder
            .Append("var originalValue = ")
            .Append("original.")
            .Append(entityProperty.PropertyName)
            .AppendLine(";");

        codeBuilder
            .Append("var currentValue = ")
            .Append("current.")
            .Append(entityProperty.PropertyName)
            .AppendLine(";");

        codeBuilder
            .AppendLine()
            .Append("if (")
            .Append("global::System.Collections.Generic.EqualityComparer<")
            .Append(entityProperty.PropertyType)
            .AppendLine(">.Default.Equals(originalValue, currentValue))")
            .AppendLine("    return;");

        codeBuilder
            .AppendLine()
            .AppendLine("var change = new global::EntityChange.ChangeRecord();");

        codeBuilder
            .Append("change.PropertyName = \"")
            .Append(entityProperty.PropertyName)
            .AppendLine("\";");

        if (!string.IsNullOrEmpty(entityProperty.DisplayName))
        {
            codeBuilder
                .Append("change.DisplayName = \"")
                .Append(entityProperty.DisplayName ?? string.Empty)
                .AppendLine("\";");
        }

        codeBuilder
            .AppendLine("change.Operation = ChangeOperation.Replace;");

        codeBuilder
            .AppendLine("change.OriginalValue = originalValue;")
            .AppendLine("change.CurrentValue = currentValue;");

        codeBuilder
            .Append("change.OriginalFormatted = global::EntityChange.NameFormatter.FormatValue(originalValue, ")
            .Append(entityProperty.StringFormat ?? "null")
            .AppendLine(");");

        codeBuilder
            .Append("change.CurrentFormatted = global::EntityChange.NameFormatter.FormatValue(currentValue, ")
            .Append(entityProperty.StringFormat ?? "null")
            .AppendLine(");");

        codeBuilder
            .AppendLine()
            .AppendLine("_changes.Add(change);");

        codeBuilder
            .DecrementIndent()
            .AppendLine("}")
            .AppendLine();
    }
}
