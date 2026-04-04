using System.Collections.Immutable;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using VerifyXunit;

using Xunit;

namespace EntityChange.Generators.Tests;

public class GeneratorSnapshotTests
{
    [Fact]
    public Task SimpleComparerGeneratesCorrectly()
    {
        var source = """
            using EntityChange;

            namespace TestApp;

            public class Order
            {
                public string Id { get; set; }
                public int OrderNumber { get; set; }
                public decimal Total { get; set; }
            }

            [GenerateComparer]
            public partial class OrderComparer : EntityComparer<Order>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task NestedObjectComparerGeneratesCorrectly()
    {
        var source = """
            using EntityChange;

            namespace TestApp;

            public class Address
            {
                public string Street { get; set; }
                public string City { get; set; }
                public string Zip { get; set; }
            }

            public class Order
            {
                public string Id { get; set; }
                public Address BillingAddress { get; set; }
                public decimal Total { get; set; }
            }

            [GenerateComparer]
            public partial class OrderComparer : EntityComparer<Order>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task CollectionPropertyGeneratesCorrectly()
    {
        var source = """
            using System.Collections.Generic;
            using EntityChange;

            namespace TestApp;

            public class OrderLine
            {
                public string Sku { get; set; }
                public int Quantity { get; set; }
            }

            public class Order
            {
                public string Id { get; set; }
                public List<OrderLine> Items { get; set; }
            }

            [GenerateComparer]
            public partial class OrderComparer : EntityComparer<Order>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task DictionaryPropertyGeneratesCorrectly()
    {
        var source = """
            using System.Collections.Generic;
            using EntityChange;

            namespace TestApp;

            public class Contact
            {
                public string Id { get; set; }
                public Dictionary<string, object> Data { get; set; }
            }

            [GenerateComparer]
            public partial class ContactComparer : EntityComparer<Contact>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task SetPropertyGeneratesCorrectly()
    {
        var source = """
            using System.Collections.Generic;
            using EntityChange;

            namespace TestApp;

            public class Contact
            {
                public string Id { get; set; }
                public HashSet<string> Categories { get; set; }
            }

            [GenerateComparer]
            public partial class ContactComparer : EntityComparer<Contact>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task CompareIgnoreAttributeExcludesProperty()
    {
        var source = """
            using EntityChange;

            namespace TestApp;

            public class Order
            {
                public string Id { get; set; }

                [CompareIgnore]
                public string InternalNote { get; set; }

                public decimal Total { get; set; }
            }

            [GenerateComparer]
            public partial class OrderComparer : EntityComparer<Order>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task RecursiveTypeGeneratesCorrectly()
    {
        var source = """
            using System.Collections.Generic;
            using EntityChange;

            namespace TestApp;

            public class TreeNode
            {
                public string Name { get; set; }
                public List<TreeNode> Children { get; set; }
            }

            [GenerateComparer]
            public partial class TreeNodeComparer : EntityComparer<TreeNode>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task InheritedPropertiesIncluded()
    {
        var source = """
            using EntityChange;

            namespace TestApp;

            public class BaseEntity
            {
                public int Id { get; set; }
            }

            public class DerivedEntity : BaseEntity
            {
                public string Name { get; set; }
            }

            [GenerateComparer]
            public partial class DerivedComparer : EntityComparer<DerivedEntity>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task DisplayAnnotationsUsed()
    {
        var source = """
            using System.ComponentModel;
            using System.ComponentModel.DataAnnotations;
            using EntityChange;

            namespace TestApp;

            public class Order
            {
                [Display(Name = "Order ID")]
                public string Id { get; set; }

                [DisplayFormat(DataFormatString = "{0:C}")]
                public decimal Total { get; set; }
            }

            [GenerateComparer]
            public partial class OrderComparer : EntityComparer<Order>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task ArrayPropertyGeneratesCorrectly()
    {
        var source = """
            using EntityChange;

            namespace TestApp;

            public class Contact
            {
                public string Id { get; set; }
                public string[] Roles { get; set; }
            }

            [GenerateComparer]
            public partial class ContactComparer : EntityComparer<Contact>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    [Fact]
    public Task AbstractPropertyUsesPolymorphicFallback()
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
                public string Name { get; set; }
            }

            [GenerateComparer]
            public partial class ConsumerComparer : EntityComparer<Consumer>
            {
            }
            """;

        return VerifyGenerator(source);
    }

    private static Task VerifyGenerator(string source)
    {
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create("TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new EntityChangeGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        return Verifier.Verify(driver).UseDirectory("Snapshots");
    }

    private static MetadataReference[] GetMetadataReferences()
    {
        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(EntityChange.IEntityComparer).Assembly,
            typeof(EntityChange.EntityComparer<>).Assembly,
            typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly,
            typeof(System.ComponentModel.DisplayNameAttribute).Assembly,
        };

        var references = assemblies
            .Select(a => a.Location)
            .Distinct()
            .Select(l => MetadataReference.CreateFromFile(l))
            .ToList();

        // Add runtime assemblies
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
