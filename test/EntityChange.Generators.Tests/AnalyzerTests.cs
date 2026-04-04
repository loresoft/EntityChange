using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using FluentAssertions;

using Xunit;

namespace EntityChange.Generators.Tests;

public class AnalyzerTests
{
    [Fact]
    public void EC0001_NonPartialClass_ReportsWarning()
    {
        var source = """
            using EntityChange;

            namespace TestApp;

            [GenerateComparer]
            public class OrderComparer : EntityComparer<Order>
            {
            }

            public class Order
            {
                public string Id { get; set; }
            }
            """;

        var diagnostics = GetAnalyzerDiagnostics(source);
        diagnostics.Should().Contain(d => d.Id == "EC0001");
    }

    [Fact]
    public void EC0002_NoDerivedFromBase_ReportsWarning()
    {
        var source = """
            using EntityChange;

            namespace TestApp;

            [GenerateComparer]
            public partial class OrderComparer
            {
            }
            """;

        var diagnostics = GetAnalyzerDiagnostics(source);
        diagnostics.Should().Contain(d => d.Id == "EC0002");
    }

    [Fact]
    public void EC0005_AbstractPropertyType_ReportsInfo()
    {
        var source = """
            using EntityChange;

            namespace TestApp;

            public abstract class AbstractBase
            {
                public int Id { get; set; }
            }

            public class Consumer
            {
                public AbstractBase SomeProperty { get; set; }
            }

            [GenerateComparer]
            public partial class ConsumerComparer : EntityComparer<Consumer>
            {
            }
            """;

        var diagnostics = GetAnalyzerDiagnostics(source);
        diagnostics.Should().Contain(d => d.Id == "EC0005");
    }

    [Fact]
    public void ValidComparer_NoWarnings()
    {
        var source = """
            using EntityChange;

            namespace TestApp;

            public class Order
            {
                public string Id { get; set; }
                public decimal Total { get; set; }
            }

            [GenerateComparer]
            public partial class OrderComparer : EntityComparer<Order>
            {
            }
            """;

        var diagnostics = GetAnalyzerDiagnostics(source);
        diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning)
            .Should().BeEmpty();
    }

    private static Diagnostic[] GetAnalyzerDiagnostics(string source)
    {
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create("TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new EntityChangeAnalyzer();

        var compilationWithAnalyzers = compilation
            .WithAnalyzers(System.Collections.Immutable.ImmutableArray.Create<Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer>(analyzer));

        return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result.ToArray();
    }

    private static MetadataReference[] GetMetadataReferences()
    {
        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(EntityChange.IEntityComparer).Assembly,
            typeof(EntityChange.EntityComparer<>).Assembly,
            typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly,
        };

        var references = assemblies
            .Select(a => a.Location)
            .Distinct()
            .Select(l => MetadataReference.CreateFromFile(l))
            .ToList();

        var runtimeDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var runtimeAssemblies = new[]
        {
            "System.Runtime.dll",
            "System.Collections.dll",
            "System.Linq.dll",
            "netstandard.dll",
        };

        foreach (var asm in runtimeAssemblies)
        {
            var path = System.IO.Path.Combine(runtimeDir, asm);
            if (System.IO.File.Exists(path))
                references.Add(MetadataReference.CreateFromFile(path));
        }

        return references.ToArray();
    }
}
