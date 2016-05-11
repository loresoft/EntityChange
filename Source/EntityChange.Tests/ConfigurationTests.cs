using System;
using System.Linq;
using EntityChange.Fluent;
using EntityChange.Tests.Models;
using FluentAssertions;
using Xunit;

namespace EntityChange.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void SimpleConfigureTest()
        {
            var configuration = new Configuration();
            var builder = new ConfigurationBuilder(configuration);

            builder.Entity<Order>(e =>
            {
                e.AutoMap();
                e.Collection(p => p.Items).CollectionComparison(CollectionComparison.ObjectEquality);
            });

            ClassMapping orderMapping;
            bool result = configuration.Mapping.TryGetValue(typeof(Order), out orderMapping);

            result.Should().BeTrue();
            orderMapping.Should().NotBeNull();


            var itemsMember = orderMapping.Members.FirstOrDefault(p => p.MemberAccessor.Name == "Items");
            itemsMember.Should().NotBeNull();
            itemsMember.CollectionComparison.Should().Be(CollectionComparison.ObjectEquality);
        }
    }
}