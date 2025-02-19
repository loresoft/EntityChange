using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

using EntityChange.Tests.Models;

using FluentAssertions;

using VerifyXunit;

using Xunit;
using Xunit.Abstractions;

namespace EntityChange.Tests;

public class EntityCompareTests
{
    private readonly ITestOutputHelper _output;

    public EntityCompareTests(ITestOutputHelper output)
    {
        _output = output;
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
    }

    [Fact]
    public async Task CompareObjectTestAsync()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
            BillingAddress = new MailingAddress
            {
                Address1 = "123 Main St",
                City = "New York",
                State = "NY",
                Zip = "10038"
            }
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
            BillingAddress = new MailingAddress
            {
                Address1 = "123 Main St",
                City = "New York",
                State = "NY",
                Zip = "10026"
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(3);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("BillingAddress.Zip");
        changes[2].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareObjectValueFormatter()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
        };

        var current = new Order
        {
            Id = original.Id,
            OrderNumber = 1000,
            Total = 11000,
        };

        var configuration = new EntityConfiguration();
        configuration.Configure(config =>
        {
            config.Entity<Order>(e =>
            {
                e.Property(p => p.Total).Formatter(d => d.ToString("C"));
            });
        });

        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(1);

        changes[0].Path.Should().Be("Total");
        changes[0].CurrentFormatted.Should().Be("$11,000.00");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareObjectNewPropertyTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
            BillingAddress = new MailingAddress
            {
                Address1 = "123 Main St",
                City = "New York",
                State = "NY",
                Zip = "10038"
            }
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
            BillingAddress = new MailingAddress
            {
                Address1 = "123 Main St",
                Address2 = "Suite 101",
                City = "New York",
                State = "NY",
                Zip = "10026"
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(4);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("BillingAddress.Address2");
        changes[2].Path.Should().Be("BillingAddress.Zip");
        changes[3].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareObjectRemovePropertyTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
            BillingAddress = new MailingAddress
            {
                Address1 = "123 Main St",
                Address2 = "Suite 101",
                City = "New York",
                State = "NY",
                Zip = "10038"
            }
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
            BillingAddress = new MailingAddress
            {
                Address1 = "123 Main St",
                City = "New York",
                State = "NY",
                Zip = "10026"
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(4);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("BillingAddress.Address2");
        changes[1].Operation.Should().Be(ChangeOperation.Replace);
        changes[2].Path.Should().Be("BillingAddress.Zip");
        changes[3].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareObjectNewObjectTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
            BillingAddress = new MailingAddress
            {
                Address1 = "123 Main St",
                City = "New York",
                State = "NY",
                Zip = "10026"
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(3);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("BillingAddress");
        changes[1].Operation.Should().Be(ChangeOperation.Replace);
        changes[2].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareObjectRemoveObjectTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
            BillingAddress = new MailingAddress
            {
                Address1 = "123 Main St",
                City = "New York",
                State = "NY",
                Zip = "10026"
            }
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(3);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("BillingAddress");
        changes[1].Operation.Should().Be(ChangeOperation.Replace);
        changes[2].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareCollectionAddItemTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "abc-123", Quanity = 1, UnitPrice = 10000 }
            }
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "abc-123", Quanity = 1, UnitPrice = 5000 },
                new OrderLine { Sku = "xyz-123", Quanity = 1, UnitPrice = 5000 }
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(4);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("Items[0].UnitPrice");
        changes[2].Path.Should().Be("Items[1]");
        changes[2].Operation.Should().Be(ChangeOperation.Add);
        changes[3].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareCollectionRemoveItemTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "abc-123", Quanity = 1, UnitPrice = 10000 },
                new OrderLine { Sku = "xyz-123", Quanity = 1, UnitPrice = 5000 }
            }
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "abc-123", Quanity = 1, UnitPrice = 5000 }
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(4);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("Items[0].UnitPrice");
        changes[2].Path.Should().Be("Items[1]");
        changes[2].Operation.Should().Be(ChangeOperation.Remove);
        changes[3].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareCollectionReplaceTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "abc-123", Quanity = 1, UnitPrice = 5000 },
            }
        };

        var configuration = new EntityConfiguration();
        configuration.Configure(config =>
        {
            config.Entity<Order>(e =>
            {
                e.Property(p => p.Id);
                e.Collection(p => p.Items).ElementFormatter(v =>
                {
                    var orderLine = v as OrderLine;
                    return orderLine?.Sku;
                });
            });
        });

        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(3);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("Items[0]");
        changes[1].CurrentFormatted.Should().Be("abc-123");
        changes[1].Operation.Should().Be(ChangeOperation.Add);
        changes[2].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareCollectionRemoveTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "abc-123", Quanity = 1, UnitPrice = 5000 },
            }
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
        };

        var configuration = new EntityConfiguration();
        configuration.Configure(config =>
        {
            config.Entity<Order>(e =>
            {
                e.Property(p => p.Id);
                e.Collection(p => p.Items).ElementFormatter(v =>
                {
                    var orderLine = v as OrderLine;
                    return orderLine?.Sku;
                });
            });
        });

        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(3);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("Items[0]");
        changes[1].OriginalFormatted.Should().Be("abc-123");
        changes[1].Operation.Should().Be(ChangeOperation.Remove);
        changes[2].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareCollectionReplaceEmptyTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
            Items = new List<OrderLine>()
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(2);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareCollectionRemoveEmptyTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 10000,
            Items = new List<OrderLine>()
        };

        var current = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = 1000,
            Total = 11000,
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(2);

        changes[0].Path.Should().Be("Id");
        changes[1].Path.Should().Be("Total");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareCollectionObjectEqualityTest()
    {
        var original = new Order
        {
            Id = Guid.NewGuid().ToString(),
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "XYZ-123", Quanity = 1, UnitPrice = 10000 },
                new OrderLine { Sku = "abc-123", Quanity = 1, UnitPrice = 10000 }
            }
        };

        var current = new Order
        {
            Id = original.Id,
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "abc-123", Quanity = 2, UnitPrice = 5000 },
                new OrderLine { Sku = "xyz-123", Quanity = 2, UnitPrice = 5000 }
            }
        };

        var configuration = new EntityConfiguration();
        configuration.Configure(config => config
            .Entity<Order>(e =>
            {
                e.Collection(p => p.Items)
                    .CollectionComparison(CollectionComparison.ObjectEquality)
                    .ElementEquality((o, c) =>
                    {
                        var l = o as OrderLine;
                        var r = c as OrderLine;

                        return string.Equals(l?.Sku, r?.Sku, StringComparison.OrdinalIgnoreCase);
                    });
            })
            .Entity<OrderLine>(e =>
            {
                e.Property(p => p.Sku).Equality(StringEquality.OrdinalIgnoreCase);
            })
        );
        var comparer = new EntityComparer(configuration);

        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(4);

        changes[0].Path.Should().Be("Items[0].Quanity");
        changes[1].Path.Should().Be("Items[0].UnitPrice");
        changes[2].Path.Should().Be("Items[1].Quanity");
        changes[3].Path.Should().Be("Items[1].UnitPrice");

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareDictionaryTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
            Data = new Dictionary<string, object>
            {
                { "Boost", 1 },
                { "Path", "./home" }
            }
        };

        var current = new Contact
        {
            Id = original.Id,
            Data = new Dictionary<string, object>
            {
                { "Boost", 2 },
                { "Path", "./path" }
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(2);

        changes[0].Path.Should().Be("Data[Boost]");
        changes[0].Operation.Should().Be(ChangeOperation.Replace);
        changes[1].Path.Should().Be("Data[Path]");
        changes[1].Operation.Should().Be(ChangeOperation.Replace);

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareDictionaryAddItemTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
            Data = new Dictionary<string, object>
            {
                { "Boost", 1 },
            }
        };

        var current = new Contact
        {
            Id = original.Id,
            Data = new Dictionary<string, object>
            {
                { "Boost", 1 },
                { "Path", "./home" }
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(1);

        changes[0].Path.Should().Be("Data[Path]");
        changes[0].Operation.Should().Be(ChangeOperation.Add);

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareDictionaryRemoveItemTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
            Data = new Dictionary<string, object>
            {
                { "Boost", 1 },
                { "Path", "./home" }
            }
        };

        var current = new Contact
        {
            Id = original.Id,
            Data = new Dictionary<string, object>
            {
                { "Boost", 1 },
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(1);

        changes[0].Path.Should().Be("Data[Path]");
        changes[0].Operation.Should().Be(ChangeOperation.Remove);

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareDictionaryReplaceTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
        };

        var current = new Contact
        {
            Id = original.Id,
            Data = new Dictionary<string, object>
            {
                { "Boost", 1 },
            }
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(1);

        changes[0].Path.Should().Be("Data[Boost]");
        changes[0].Operation.Should().Be(ChangeOperation.Add);

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareDictionaryRemoveTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
            Data = new Dictionary<string, object>
            {
                { "Boost", 1 },
            }
        };

        var current = new Contact
        {
            Id = original.Id,
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(1);

        changes[0].Path.Should().Be("Data[Boost]");
        changes[0].Operation.Should().Be(ChangeOperation.Remove);

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareDictionaryReplaceEmptyTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
        };

        var current = new Contact
        {
            Id = original.Id,
            Data = new Dictionary<string, object>()
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(0);

        WriteMarkdown(changes);


        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareDictionaryRemoveEmptyTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
            Data = new Dictionary<string, object>()
        };

        var current = new Contact
        {
            Id = original.Id,
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(0);

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareSetAddItemTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
            Categories = new HashSet<string> { "Person", "Owner" },
        };

        var current = new Contact
        {
            Id = original.Id,
            Categories = new HashSet<string> { "Person", "Owner", "Blah" },
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(1);

        changes[0].Path.Should().Be("Categories[2]");
        changes[0].Operation.Should().Be(ChangeOperation.Add);

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareSetRemoveItemTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
            Categories = new HashSet<string> { "Person", "Owner", "Blah" },
        };

        var current = new Contact
        {
            Id = original.Id,
            Categories = new HashSet<string> { "Person", "Owner" },
        };

        var configuration = new EntityConfiguration();
        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(1);

        changes[0].Path.Should().Be("Categories[2]");
        changes[0].Operation.Should().Be(ChangeOperation.Remove);

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareComplexCompareTest()
    {
        var original = new Contact
        {
            Id = Guid.NewGuid().ToString(),
            Created = new DateTime(2024, 1, 6),
            Updated = new DateTime(2024, 1, 6),
            FirstName = "Jim",
            LastName = "Bob",
            JobTitle = "CEO",
            Status = Status.New,
            Roles = new[] { "Administrator", "User" },
            Categories = new HashSet<string> { "Person", "Owner" },
            Data = new Dictionary<string, object>
            {
                { "Boost", 1 },
                { "Path", "./home" }
            },
            EmailAddresses = new List<EmailAddress>
            {
                new EmailAddress { Address = "user@Business.com", Type = ContactType.Business },
                new EmailAddress { Address = "user@Personal.com", Type = ContactType.Personal },
            },
            MailingAddresses = new List<MailingAddress>
            {
                new MailingAddress
                {
                    Address1 = "123 Main St",
                    City = "New York",
                    State = "NY",
                    Zip = "10026"
                }
            },
            PhoneNumbers = new List<PhoneNumber>
            {
                new PhoneNumber { Number ="888-555-1212", Type = ContactType.Business }
            }
        };

        var current = new Contact
        {
            Id = original.Id,
            Created = new DateTime(2024, 1, 6),
            Updated = new DateTime(2024, 1, 7),
            FirstName = "Jim",
            LastName = "Bob",
            JobTitle = "CEO",
            Status = Status.Verified,
            Roles = new[] { "User" },
            Categories = new HashSet<string> { "Person", "Owner", "Blah" },
            Data = new Dictionary<string, object>
            {
                { "Boost", 2 },
                { "Path", "./path" }
            },
            EmailAddresses = new List<EmailAddress>
            {
                new EmailAddress { Address = "user@Business.com", Type = ContactType.Business },
                new EmailAddress { Address = "user@gmail.com", Type = ContactType.Personal },
                new EmailAddress { Address = "user@home.com", Type = ContactType.Home },
            },
            MailingAddresses = new List<MailingAddress>
            {
                new MailingAddress
                {
                    Address1 = "123 Main St",
                    City = "New York",
                    State = "NY",
                    Zip = "10027"
                }
            },
            PhoneNumbers = new List<PhoneNumber>
            {
                new PhoneNumber { Number ="800-555-1212", Type = ContactType.Business }
            }
        };

        var configuration = new EntityConfiguration();
        configuration.Configure(config => config
            .Entity<Contact>(e =>
            {
                e.Property(p => p.Created).Formatter(p => p.ToShortDateString());
                e.Property(p => p.Updated).Formatter(p => p.ToShortDateString());
                e.Property(p => p.FirstName).Display("First Name");
                e.Collection(p => p.Roles)
                    .CollectionComparison(CollectionComparison.ObjectEquality)
                    .ElementEquality(StringEquality.OrdinalIgnoreCase);
                e.Collection(p => p.EmailAddresses).ElementFormatter(v =>
                {
                    var address = v as EmailAddress;
                    return address?.Address;
                });
            })
            .Entity<EmailAddress>(e =>
            {
                e.Property(p => p.Address).Display("Email Address");
            })
        );

        var comparer = new EntityComparer(configuration);
        var changes = comparer.Compare(original, current);

        changes.Should().NotBeNull();
        changes.Count.Should().Be(10);

        WriteMarkdown(changes);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareNestedObjectsPathsTest()
    {
        var node = new TreeNode
        {
            Name = "Root",
            nodes = new List<TreeNode>
            {
                new TreeNode
                {
                    Name = "Level 1",
                    nodes = new List<TreeNode>
                    {
                        new TreeNode
                        {
                            Name = "Level 2"
                        }
                    }
                }
            }
        };

        var node2 = new TreeNode
        {
            Name = "Root",
            nodes = new List<TreeNode>
            {
                new TreeNode
                {
                    Name = "Level 1",
                    nodes = new List<TreeNode>
                    {
                        new TreeNode
                        {
                            Name = "Level 3"
                        }
                    }
                }
            }
        };

        EntityComparer entityComparer = new EntityComparer();
        var changes = entityComparer.Compare(node, node2);

        changes.Should().NotBeEmpty();
        changes.First().Path.Should().Be("nodes[0].nodes[0].Name");

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareValueTypeRootElementsTest()
    {
        int original = 1;
        int current = 2;

        var entityComparer = new EntityComparer();
        var changes = entityComparer.Compare(original, current);

        changes.Should().NotBeEmpty();

        ChangeRecord changeRecord = changes.First();
        changeRecord.OriginalValue.Should().Be(1);
        changeRecord.CurrentValue.Should().Be(2);

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareArrayRootElementsTest()
    {
        TreeNode[] original = [new TreeNode { Name = "Level 1" }];
        TreeNode[] current = [];

        EntityComparer entityComparer = new EntityComparer();
        var changes = entityComparer.Compare(original, current);

        changes.Should().NotBeEmpty();

        ChangeRecord changeRecord = changes.First();
        changeRecord.Path.Should().Be("[0]");

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task CompareAbstract()
    {
        var obj1 = new Consumer()
        {
            SomeProperty = new ConcreteClass()
            {
                Id = 1,
                SomeString = "Test1"
            }
        };
        var obj2 = new Consumer()
        {
            SomeProperty = new ConcreteClass()
            {
                Id = 2,
                SomeString = "Test2"
            }
        };

        var comparer = new EntityComparer();
        var changes = comparer.Compare(obj1, obj2);

        changes.Should().NotBeEmpty();

        await Verifier.Verify(changes).UseDirectory("Snapshots");
    }



    private void WriteMarkdown(IReadOnlyList<ChangeRecord> changes)
    {
        var formatter = new MarkdownFormatter();
        var markdown = formatter.Format(changes);

        _output.WriteLine(markdown);
    }
}
