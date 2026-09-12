using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using EntityChange.Tests.Models;

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
    public void CompareObjectTestAsync()
    {
        var original = new Order
        {
            Id = "00000001-0000-0000-0000-000000000000",
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
            Id = "00000002-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000001-0000-0000-0000-000000000000",
                "currentValue": "00000002-0000-0000-0000-000000000000",
                "originalFormatted": "00000001-0000-0000-0000-000000000000",
                "currentFormatted": "00000002-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "Zip",
                "displayName": "Zip",
                "path": "BillingAddress.Zip",
                "operation": "Replace",
                "originalValue": "10038",
                "currentValue": "10026",
                "originalFormatted": "10038",
                "currentFormatted": "10026"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareObjectValueFormatter()
    {
        var original = new Order
        {
            Id = "00000003-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "$10,000.00",
                "currentFormatted": "$11,000.00"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareObjectNewPropertyTest()
    {
        var original = new Order
        {
            Id = "00000004-0000-0000-0000-000000000000",
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
            Id = "00000005-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000004-0000-0000-0000-000000000000",
                "currentValue": "00000005-0000-0000-0000-000000000000",
                "originalFormatted": "00000004-0000-0000-0000-000000000000",
                "currentFormatted": "00000005-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "Address2",
                "displayName": "Address 2",
                "path": "BillingAddress.Address2",
                "operation": "Replace",
                "currentValue": "Suite 101",
                "currentFormatted": "Suite 101"
              },
              {
                "propertyName": "Zip",
                "displayName": "Zip",
                "path": "BillingAddress.Zip",
                "operation": "Replace",
                "originalValue": "10038",
                "currentValue": "10026",
                "originalFormatted": "10038",
                "currentFormatted": "10026"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareObjectRemovePropertyTest()
    {
        var original = new Order
        {
            Id = "00000006-0000-0000-0000-000000000000",
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
            Id = "00000007-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000006-0000-0000-0000-000000000000",
                "currentValue": "00000007-0000-0000-0000-000000000000",
                "originalFormatted": "00000006-0000-0000-0000-000000000000",
                "currentFormatted": "00000007-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "Address2",
                "displayName": "Address 2",
                "path": "BillingAddress.Address2",
                "operation": "Replace",
                "originalValue": "Suite 101",
                "originalFormatted": "Suite 101"
              },
              {
                "propertyName": "Zip",
                "displayName": "Zip",
                "path": "BillingAddress.Zip",
                "operation": "Replace",
                "originalValue": "10038",
                "currentValue": "10026",
                "originalFormatted": "10038",
                "currentFormatted": "10026"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareObjectNewObjectTest()
    {
        var original = new Order
        {
            Id = "00000008-0000-0000-0000-000000000000",
            OrderNumber = 1000,
            Total = 10000
        };

        var current = new Order
        {
            Id = "00000009-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000008-0000-0000-0000-000000000000",
                "currentValue": "00000009-0000-0000-0000-000000000000",
                "originalFormatted": "00000008-0000-0000-0000-000000000000",
                "currentFormatted": "00000009-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "BillingAddress",
                "displayName": "Billing Address",
                "path": "BillingAddress",
                "operation": "Replace",
                "currentValue": "123 Main St, New York, NY 10026",
                "currentFormatted": "123 Main St, New York, NY 10026"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareObjectRemoveObjectTest()
    {
        var original = new Order
        {
            Id = "00000010-0000-0000-0000-000000000000",
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
            Id = "00000011-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000010-0000-0000-0000-000000000000",
                "currentValue": "00000011-0000-0000-0000-000000000000",
                "originalFormatted": "00000010-0000-0000-0000-000000000000",
                "currentFormatted": "00000011-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "BillingAddress",
                "displayName": "Billing Address",
                "path": "BillingAddress",
                "operation": "Replace",
                "originalValue": "123 Main St, New York, NY 10026",
                "originalFormatted": "123 Main St, New York, NY 10026"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareCollectionAddItemTest()
    {
        var original = new Order
        {
            Id = "00000012-0000-0000-0000-000000000000",
            OrderNumber = 1000,
            Total = 10000,
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "abc-123", Quanity = 1, UnitPrice = 10000 }
            }
        };

        var current = new Order
        {
            Id = "00000013-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000012-0000-0000-0000-000000000000",
                "currentValue": "00000013-0000-0000-0000-000000000000",
                "originalFormatted": "00000012-0000-0000-0000-000000000000",
                "currentFormatted": "00000013-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "UnitPrice",
                "displayName": "Unit Price",
                "path": "Items[0].UnitPrice",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 5000,
                "originalFormatted": "10000",
                "currentFormatted": "5000"
              },
              {
                "propertyName": "Items[1]",
                "displayName": "Items",
                "path": "Items[1]",
                "operation": "Add",
                "currentValue": "EntityChange.Tests.Models.OrderLine",
                "currentFormatted": "EntityChange.Tests.Models.OrderLine"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareCollectionRemoveItemTest()
    {
        var original = new Order
        {
            Id = "00000014-0000-0000-0000-000000000000",
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
            Id = "00000015-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000014-0000-0000-0000-000000000000",
                "currentValue": "00000015-0000-0000-0000-000000000000",
                "originalFormatted": "00000014-0000-0000-0000-000000000000",
                "currentFormatted": "00000015-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "UnitPrice",
                "displayName": "Unit Price",
                "path": "Items[0].UnitPrice",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 5000,
                "originalFormatted": "10000",
                "currentFormatted": "5000"
              },
              {
                "propertyName": "Items[1]",
                "displayName": "Items",
                "path": "Items[1]",
                "operation": "Remove",
                "originalValue": "EntityChange.Tests.Models.OrderLine",
                "originalFormatted": "EntityChange.Tests.Models.OrderLine"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareCollectionReplaceTest()
    {
        var original = new Order
        {
            Id = "00000016-0000-0000-0000-000000000000",
            OrderNumber = 1000,
            Total = 10000,
        };

        var current = new Order
        {
            Id = "00000017-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000016-0000-0000-0000-000000000000",
                "currentValue": "00000017-0000-0000-0000-000000000000",
                "originalFormatted": "00000016-0000-0000-0000-000000000000",
                "currentFormatted": "00000017-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "Items[0]",
                "displayName": "Items",
                "path": "Items[0]",
                "operation": "Add",
                "currentValue": "EntityChange.Tests.Models.OrderLine",
                "currentFormatted": "abc-123"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareCollectionRemoveTest()
    {
        var original = new Order
        {
            Id = "00000018-0000-0000-0000-000000000000",
            OrderNumber = 1000,
            Total = 10000,
            Items = new List<OrderLine>
            {
                new OrderLine { Sku = "abc-123", Quanity = 1, UnitPrice = 5000 },
            }
        };

        var current = new Order
        {
            Id = "00000019-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000018-0000-0000-0000-000000000000",
                "currentValue": "00000019-0000-0000-0000-000000000000",
                "originalFormatted": "00000018-0000-0000-0000-000000000000",
                "currentFormatted": "00000019-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "Items[0]",
                "displayName": "Items",
                "path": "Items[0]",
                "operation": "Remove",
                "originalValue": "EntityChange.Tests.Models.OrderLine",
                "originalFormatted": "abc-123"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareCollectionReplaceEmptyTest()
    {
        var original = new Order
        {
            Id = "00000020-0000-0000-0000-000000000000",
            OrderNumber = 1000,
            Total = 10000,
        };

        var current = new Order
        {
            Id = "00000021-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000020-0000-0000-0000-000000000000",
                "currentValue": "00000021-0000-0000-0000-000000000000",
                "originalFormatted": "00000020-0000-0000-0000-000000000000",
                "currentFormatted": "00000021-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareCollectionRemoveEmptyTest()
    {
        var original = new Order
        {
            Id = "00000022-0000-0000-0000-000000000000",
            OrderNumber = 1000,
            Total = 10000,
            Items = new List<OrderLine>()
        };

        var current = new Order
        {
            Id = "00000023-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "Id",
                "operation": "Replace",
                "originalValue": "00000022-0000-0000-0000-000000000000",
                "currentValue": "00000023-0000-0000-0000-000000000000",
                "originalFormatted": "00000022-0000-0000-0000-000000000000",
                "currentFormatted": "00000023-0000-0000-0000-000000000000"
              },
              {
                "propertyName": "Total",
                "displayName": "Total",
                "path": "Total",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 11000,
                "originalFormatted": "10000",
                "currentFormatted": "11000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareCollectionObjectEqualityTest()
    {
        var original = new Order
        {
            Id = "00000024-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Quanity",
                "displayName": "Quanity",
                "path": "Items[0].Quanity",
                "operation": "Replace",
                "originalValue": 1,
                "currentValue": 2,
                "originalFormatted": "1",
                "currentFormatted": "2"
              },
              {
                "propertyName": "UnitPrice",
                "displayName": "Unit Price",
                "path": "Items[0].UnitPrice",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 5000,
                "originalFormatted": "10000",
                "currentFormatted": "5000"
              },
              {
                "propertyName": "Quanity",
                "displayName": "Quanity",
                "path": "Items[1].Quanity",
                "operation": "Replace",
                "originalValue": 1,
                "currentValue": 2,
                "originalFormatted": "1",
                "currentFormatted": "2"
              },
              {
                "propertyName": "UnitPrice",
                "displayName": "Unit Price",
                "path": "Items[1].UnitPrice",
                "operation": "Replace",
                "originalValue": 10000,
                "currentValue": 5000,
                "originalFormatted": "10000",
                "currentFormatted": "5000"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareDictionaryTest()
    {
        var original = new Contact
        {
            Id = "00000025-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Data[Boost]",
                "displayName": "Data",
                "path": "Data[Boost]",
                "operation": "Replace",
                "originalValue": 1,
                "currentValue": 2,
                "originalFormatted": "1",
                "currentFormatted": "2"
              },
              {
                "propertyName": "Data[Path]",
                "displayName": "Data",
                "path": "Data[Path]",
                "operation": "Replace",
                "originalValue": "./home",
                "currentValue": "./path",
                "originalFormatted": "./home",
                "currentFormatted": "./path"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareDictionaryAddItemTest()
    {
        var original = new Contact
        {
            Id = "00000026-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Data[Path]",
                "displayName": "Data",
                "path": "Data[Path]",
                "operation": "Add",
                "currentValue": "./home",
                "currentFormatted": "./home"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareDictionaryRemoveItemTest()
    {
        var original = new Contact
        {
            Id = "00000027-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Data[Path]",
                "displayName": "Data",
                "path": "Data[Path]",
                "operation": "Remove",
                "originalValue": "./home",
                "originalFormatted": "./home"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareDictionaryReplaceTest()
    {
        var original = new Contact
        {
            Id = "00000028-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Data[Boost]",
                "displayName": "Data",
                "path": "Data[Boost]",
                "operation": "Add",
                "currentValue": 1,
                "currentFormatted": "1"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareDictionaryRemoveTest()
    {
        var original = new Contact
        {
            Id = "00000029-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Data[Boost]",
                "displayName": "Data",
                "path": "Data[Boost]",
                "operation": "Remove",
                "originalValue": 1,
                "originalFormatted": "1"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareDictionaryReplaceEmptyTest()
    {
        var original = new Contact
        {
            Id = "00000030-0000-0000-0000-000000000000",
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


        const string expected =
            """
            []
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareDictionaryRemoveEmptyTest()
    {
        var original = new Contact
        {
            Id = "00000031-0000-0000-0000-000000000000",
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

        const string expected =
            """
            []
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareSetAddItemTest()
    {
        var original = new Contact
        {
            Id = "00000032-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Categories[2]",
                "displayName": "Categories",
                "path": "Categories[2]",
                "operation": "Add",
                "currentValue": "Blah",
                "currentFormatted": "Blah"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareSetRemoveItemTest()
    {
        var original = new Contact
        {
            Id = "00000033-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Categories[2]",
                "displayName": "Categories",
                "path": "Categories[2]",
                "operation": "Remove",
                "originalValue": "Blah",
                "originalFormatted": "Blah"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareComplexCompareTest()
    {
        var original = new Contact
        {
            Id = "00000034-0000-0000-0000-000000000000",
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

        const string expected =
            """
            [
              {
                "propertyName": "Updated",
                "displayName": "Updated",
                "path": "Updated",
                "operation": "Replace",
                "originalValue": "2024-01-06T00:00:00",
                "currentValue": "2024-01-07T00:00:00",
                "originalFormatted": "1/6/2024",
                "currentFormatted": "1/7/2024"
              },
              {
                "propertyName": "Roles",
                "displayName": "Roles",
                "path": "Roles",
                "operation": "Remove",
                "originalValue": "Administrator",
                "originalFormatted": "Administrator"
              },
              {
                "propertyName": "Address",
                "displayName": "Email Address",
                "path": "EmailAddresses[1].Address",
                "operation": "Replace",
                "originalValue": "user@Personal.com",
                "currentValue": "user@gmail.com",
                "originalFormatted": "user@Personal.com",
                "currentFormatted": "user@gmail.com"
              },
              {
                "propertyName": "EmailAddresses[2]",
                "displayName": "Email Addresses",
                "path": "EmailAddresses[2]",
                "operation": "Add",
                "currentValue": "EntityChange.Tests.Models.EmailAddress",
                "currentFormatted": "user@home.com"
              },
              {
                "propertyName": "Status",
                "displayName": "Status",
                "path": "Status",
                "operation": "Replace",
                "originalValue": "New",
                "currentValue": "Verified",
                "originalFormatted": "New",
                "currentFormatted": "Verified"
              },
              {
                "propertyName": "Zip",
                "displayName": "Zip",
                "path": "MailingAddresses[0].Zip",
                "operation": "Replace",
                "originalValue": "10026",
                "currentValue": "10027",
                "originalFormatted": "10026",
                "currentFormatted": "10027"
              },
              {
                "propertyName": "Number",
                "displayName": "Number",
                "path": "PhoneNumbers[0].Number",
                "operation": "Replace",
                "originalValue": "888-555-1212",
                "currentValue": "800-555-1212",
                "originalFormatted": "888-555-1212",
                "currentFormatted": "800-555-1212"
              },
              {
                "propertyName": "Categories[2]",
                "displayName": "Categories",
                "path": "Categories[2]",
                "operation": "Add",
                "currentValue": "Blah",
                "currentFormatted": "Blah"
              },
              {
                "propertyName": "Data[Boost]",
                "displayName": "Data",
                "path": "Data[Boost]",
                "operation": "Replace",
                "originalValue": 1,
                "currentValue": 2,
                "originalFormatted": "1",
                "currentFormatted": "2"
              },
              {
                "propertyName": "Data[Path]",
                "displayName": "Data",
                "path": "Data[Path]",
                "operation": "Replace",
                "originalValue": "./home",
                "currentValue": "./path",
                "originalFormatted": "./home",
                "currentFormatted": "./path"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareNestedObjectsPathsTest()
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

        const string expected =
            """
            [
              {
                "propertyName": "Name",
                "displayName": "Name",
                "path": "nodes[0].nodes[0].Name",
                "operation": "Replace",
                "originalValue": "Level 2",
                "currentValue": "Level 3",
                "originalFormatted": "Level 2",
                "currentFormatted": "Level 3"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareValueTypeRootElementsTest()
    {
        int original = 1;
        int current = 2;

        var entityComparer = new EntityComparer();
        var changes = entityComparer.Compare(original, current);

        changes.Should().NotBeEmpty();

        ChangeRecord changeRecord = changes.First();
        changeRecord.OriginalValue.Should().Be(1);
        changeRecord.CurrentValue.Should().Be(2);

        const string expected =
            """
            [
              {
                "propertyName": "",
                "displayName": "",
                "path": "",
                "operation": "Replace",
                "originalValue": 1,
                "currentValue": 2,
                "originalFormatted": "1",
                "currentFormatted": "2"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareArrayRootElementsTest()
    {
        TreeNode[] original = [new TreeNode { Name = "Level 1" }];
        TreeNode[] current = [];

        EntityComparer entityComparer = new EntityComparer();
        var changes = entityComparer.Compare(original, current);

        changes.Should().NotBeEmpty();

        ChangeRecord changeRecord = changes.First();
        changeRecord.Path.Should().Be("[0]");

        const string expected =
            """
            [
              {
                "propertyName": "[0]",
                "displayName": "0",
                "path": "[0]",
                "operation": "Remove",
                "originalValue": "EntityChange.Tests.Models.TreeNode",
                "originalFormatted": "EntityChange.Tests.Models.TreeNode"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }

    [Fact]
    public void CompareAbstract()
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

        const string expected =
            """
            [
              {
                "propertyName": "SomeString",
                "displayName": "Some String",
                "path": "SomeProperty.SomeString",
                "operation": "Replace",
                "originalValue": "Test1",
                "currentValue": "Test2",
                "originalFormatted": "Test1",
                "currentFormatted": "Test2"
              },
              {
                "propertyName": "Id",
                "displayName": "Id",
                "path": "SomeProperty.Id",
                "operation": "Replace",
                "originalValue": 1,
                "currentValue": 2,
                "originalFormatted": "1",
                "currentFormatted": "2"
              }
            ]
            """;

        JsonAssert.Equal(expected, changes);
    }



    private void WriteMarkdown(IReadOnlyList<ChangeRecord> changes)
    {
        var formatter = new MarkdownFormatter();
        var markdown = formatter.Format(changes);

        _output.WriteLine(markdown);
    }
}
