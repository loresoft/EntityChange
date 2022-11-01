using System.Globalization;

using EntityChange.Tests.Models;

using FluentAssertions;

using Xunit;

namespace EntityChange.Tests;

public class StringFomatterTests
{
    public StringFomatterTests()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
    }

    [Fact]
    public void NumberFormat()
    {
        var sample = DataType.Sample();

        var formatted = StringFormatter.Number(sample.Double);
        formatted.Should().Be("1,032.50");

        formatted = StringFormatter.Number(sample.Decimal);
        formatted.Should().Be("12,000.25");

        formatted = StringFormatter.Number(sample.Long);
        formatted.Should().Be("100,042.00");
    }

    [Fact]
    public void CurrencyFormat()
    {
        var sample = DataType.Sample();

        var formatted = StringFormatter.Currency(sample.Double);
        formatted.Should().Be("$1,032.50");

        formatted = StringFormatter.Currency(sample.Decimal);
        formatted.Should().Be("$12,000.25");

        formatted = StringFormatter.Currency(sample.Long);
        formatted.Should().Be("$100,042.00");
    }
}