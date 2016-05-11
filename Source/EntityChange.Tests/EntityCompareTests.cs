using System;
using System.Collections.Generic;
using System.Linq;
using EntityChange.Tests.Models;
using FluentAssertions;
using Xunit;

namespace EntityChange.Tests
{
    public class EntityCompareTests
    {
        [Fact]
        public void CompareObjectTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(3);

            changes[0].Path.Should().Be("Id");
            changes[1].Path.Should().Be("BillingAddress.Zip");
            changes[2].Path.Should().Be("Total");

        }

        [Fact]
        public void CompareObjectNewPropertyTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(4);

            changes[0].Path.Should().Be("Id");
            changes[1].Path.Should().Be("BillingAddress.Address2");
            changes[2].Path.Should().Be("BillingAddress.Zip");
            changes[3].Path.Should().Be("Total");

        }

        [Fact]
        public void CompareObjectRemovePropertyTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(4);

            changes[0].Path.Should().Be("Id");
            changes[1].Path.Should().Be("BillingAddress.Address2");
            changes[1].Operation.Should().Be(ChangeOperation.Replace);
            changes[2].Path.Should().Be("BillingAddress.Zip");
            changes[3].Path.Should().Be("Total");

        }

        [Fact]
        public void CompareObjectNewObjectTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(3);

            changes[0].Path.Should().Be("Id");
            changes[1].Path.Should().Be("BillingAddress");
            changes[2].Path.Should().Be("Total");

        }

        [Fact]
        public void CompareObjectRemoveObjectTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(3);

            changes[0].Path.Should().Be("Id");
            changes[1].Path.Should().Be("BillingAddress");
            changes[1].Operation.Should().Be(ChangeOperation.Remove);
            changes[2].Path.Should().Be("Total");

        }


        [Fact]
        public void CompareCollectionAddItemTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(4);

            changes[0].Path.Should().Be("Id");
            changes[1].Path.Should().Be("Items[0].UnitPrice");
            changes[2].Path.Should().Be("Items[1]");
            changes[2].Operation.Should().Be(ChangeOperation.Add);
            changes[3].Path.Should().Be("Total");

        }

        [Fact]
        public void CompareCollectionRemoveItemTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(4);

            changes[0].Path.Should().Be("Id");
            changes[1].Path.Should().Be("Items[0].UnitPrice");
            changes[2].Path.Should().Be("Items[1]");
            changes[2].Operation.Should().Be(ChangeOperation.Remove);
            changes[3].Path.Should().Be("Total");
        }

        [Fact]
        public void CompareCollectionReplaceTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(3);

            changes[0].Path.Should().Be("Id");
            changes[1].Path.Should().Be("Items");
            changes[1].Operation.Should().Be(ChangeOperation.Replace);
            changes[2].Path.Should().Be("Total");

        }

        [Fact]
        public void CompareCollectionRemoveTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(3);

            changes[0].Path.Should().Be("Id");
            changes[1].Path.Should().Be("Items");
            changes[1].Operation.Should().Be(ChangeOperation.Remove);
            changes[2].Path.Should().Be("Total");
        }

        [Fact]
        public void CompareCollectionObjectEqualityTest()
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

            var comparer = new EntityComparer();
            comparer.Configure(config => config
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


            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(4);

            changes[0].Path.Should().Be("Items[0].Quanity");
            changes[1].Path.Should().Be("Items[0].UnitPrice");
            changes[2].Path.Should().Be("Items[1].Quanity");
            changes[3].Path.Should().Be("Items[1].UnitPrice");
        }


        [Fact]
        public void CompareDictionaryTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(2);

            changes[0].Path.Should().Be("Data[Boost]");
            changes[0].Operation.Should().Be(ChangeOperation.Replace);
            changes[1].Path.Should().Be("Data[Path]");
            changes[1].Operation.Should().Be(ChangeOperation.Replace);
        }

        [Fact]
        public void CompareDictionaryAddItemTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(1);

            changes[0].Path.Should().Be("Data[Path]");
            changes[0].Operation.Should().Be(ChangeOperation.Add);
        }

        [Fact]
        public void CompareDictionaryRemoveItemTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(1);

            changes[0].Path.Should().Be("Data[Path]");
            changes[0].Operation.Should().Be(ChangeOperation.Remove);
        }

        [Fact]
        public void CompareDictionaryReplaceTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(1);

            changes[0].Path.Should().Be("Data");
            changes[0].Operation.Should().Be(ChangeOperation.Replace);
        }

        [Fact]
        public void CompareDictionaryRemoveTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(1);

            changes[0].Path.Should().Be("Data");
            changes[0].Operation.Should().Be(ChangeOperation.Remove);
        }


        [Fact]
        public void CompareSetAddItemTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(1);

            changes[0].Path.Should().Be("Categories[2]");
            changes[0].Operation.Should().Be(ChangeOperation.Add);
        }

        [Fact]
        public void CompareSetRemoveItemTest()
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

            var comparer = new EntityComparer();
            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(1);

            changes[0].Path.Should().Be("Categories[2]");
            changes[0].Operation.Should().Be(ChangeOperation.Remove);
        }


        [Fact]
        public void CompareComplexCompareTest()
        {
            var original = new Contact
            {
                Id = Guid.NewGuid().ToString(),
                Created = DateTime.Now,
                Updated = DateTime.Now,
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
                Created = original.Created,
                Updated = DateTime.Now,
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

            var comparer = new EntityComparer();

            comparer.Configure(config => config
                .Entity<Contact>(e =>
                {
                    e.Property(p => p.FirstName);
                    e.Collection(p => p.Roles)
                        .CollectionComparison(CollectionComparison.ObjectEquality)
                        .ElementEquality(StringEquality.OrdinalIgnoreCase);
                })
            );

            var changes = comparer.Compare(original, current).ToList();

            changes.Should().NotBeNull();
            changes.Count.Should().Be(9);

        }
    }



}
