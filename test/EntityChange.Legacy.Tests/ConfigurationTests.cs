using System.Linq;

using EntityChange.Fluent;
using EntityChange.Tests.Models;

using FluentAssertions;

using Xunit;

namespace EntityChange.Tests;

public class ConfigurationTests
{
    [Fact]
    public void SimpleConfigureTest()
    {
        var configuration = new EntityConfiguration();
        var builder = new ConfigurationBuilder(configuration);

        builder.Entity<Order>(e =>
        {
            e.AutoMap();
            e.Collection(p => p.Items).CollectionComparison(CollectionComparison.ObjectEquality);
        });

        EntityMapping orderMapping;
        bool result = configuration.Mapping.TryGetValue(typeof(Order), out orderMapping);

        result.Should().BeTrue();
        orderMapping.Should().NotBeNull();


        var itemsMember = orderMapping.Members.FirstOrDefault(p => p.MemberAccessor.Name == "Items");
        itemsMember.Should().NotBeNull();
        itemsMember.CollectionComparison.Should().Be(CollectionComparison.ObjectEquality);
    }

    [Fact]
    public void AutoMapDefaultOffTest()
    {
        var configuration = new EntityConfiguration();
        var builder = new ConfigurationBuilder(configuration);
        builder.AutoMap(false);
        builder.Entity<EmailAddress>(e => e.AutoMap());


        var orderMapping = configuration.GetMapping(typeof(Order));
        orderMapping.Should().NotBeNull();
        orderMapping.AutoMap.Should().BeFalse();
        orderMapping.Members.Count.Should().Be(0);

        var emailMapping = configuration.GetMapping(typeof(EmailAddress));
        emailMapping.Should().NotBeNull();
        emailMapping.AutoMap.Should().BeTrue();
        emailMapping.Members.Count.Should().Be(2);


    }

    [Fact]
    public void AutoMapDefaultOnTest()
    {
        var configuration = new EntityConfiguration();
        var builder = new ConfigurationBuilder(configuration);
        builder.Entity<Order>(e => e.AutoMap(false));

        var orderMapping = configuration.GetMapping(typeof(Order));
        orderMapping.Should().NotBeNull();
        orderMapping.AutoMap.Should().BeFalse();
        orderMapping.Members.Count.Should().Be(0);

        var emailMapping = configuration.GetMapping(typeof(EmailAddress));
        emailMapping.Should().NotBeNull();
        emailMapping.AutoMap.Should().BeTrue();
        emailMapping.Members.Count.Should().Be(2);


    }
}
