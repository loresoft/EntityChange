using FluentAssertions;

using Xunit;

namespace EntityChange.Tests;

public class PathStackTests
{
    [Fact]
    public void EmptyTest()
    {
        var pathStack = new PathStack();
        var path = pathStack.ToString();

        path.Should().BeEmpty();
    }

    [Fact]
    public void EmptyPopTest()
    {
        var pathStack = new PathStack();
        pathStack.Pop();

        var path = pathStack.ToString();

        path.Should().BeEmpty();
    }

    [Fact]
    public void FirstIndexerTest()
    {
        var pathStack = new PathStack();
        pathStack.PushIndex(1);

        var name = pathStack.CurrentName();
        name.Should().Be("[1]");

        var property = pathStack.CurrentProperty();
        property.Should().BeEmpty();

        var path = pathStack.ToString();
        path.Should().Be("[1]");
    }

    [Fact]
    public void FirstPropertyTest()
    {
        var pathStack = new PathStack();
        pathStack.PushProperty("Name");

        var name = pathStack.CurrentName();
        name.Should().Be("Name");

        var property = pathStack.CurrentProperty();
        property.Should().Be("Name");

        var path = pathStack.ToString();
        path.Should().Be("Name");
    }

    [Fact]
    public void NestedPropertyTest()
    {
        var pathStack = new PathStack();
        pathStack.PushProperty("HomeAddress");
        pathStack.PushProperty("AddressLine1");

        var name = pathStack.CurrentName();
        name.Should().Be("AddressLine1");

        var property = pathStack.CurrentProperty();
        property.Should().Be("AddressLine1");

        var path = pathStack.ToString();
        path.Should().Be("HomeAddress.AddressLine1");

        pathStack.Pop();

        name = pathStack.CurrentName();
        name.Should().Be("HomeAddress");

        property = pathStack.CurrentProperty();
        property.Should().Be("HomeAddress");

        path = pathStack.ToString();
        path.Should().Be("HomeAddress");
    }

    [Fact]
    public void PropertyIndexerTest()
    {
        var pathStack = new PathStack();
        pathStack.PushProperty("Items");
        pathStack.PushIndex(0);

        var name = pathStack.CurrentName();
        name.Should().Be("Items[0]");

        var property = pathStack.CurrentProperty();
        property.Should().Be("Items");

        var path = pathStack.ToString();
        path.Should().Be("Items[0]");

        pathStack.Pop();

        pathStack.PushIndex(1);

        name = pathStack.CurrentName();
        name.Should().Be("Items[1]");

        property = pathStack.CurrentProperty();
        property.Should().Be("Items");

        path = pathStack.ToString();
        path.Should().Be("Items[1]");

        pathStack.PushProperty("Name");

        name = pathStack.CurrentName();
        name.Should().Be("Name");

        property = pathStack.CurrentProperty();
        property.Should().Be("Name");

        path = pathStack.ToString();
        path.Should().Be("Items[1].Name");
    }

    [Fact]
    public void PropertyIndexerDoubleTest()
    {
        var pathStack = new PathStack();
        pathStack.PushProperty("Items");
        pathStack.PushIndex(0);

        var name = pathStack.CurrentName();
        name.Should().Be("Items[0]");

        var property = pathStack.CurrentProperty();
        property.Should().Be("Items");

        var path = pathStack.ToString();
        path.Should().Be("Items[0]");

        pathStack.PushIndex(0);

        name = pathStack.CurrentName();
        name.Should().Be("Items[0][0]");

        property = pathStack.CurrentProperty();
        property.Should().Be("Items");

        path = pathStack.ToString();
        path.Should().Be("Items[0][0]");
    }

}
