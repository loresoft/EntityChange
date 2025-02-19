namespace EntityChange.Tests.Models;

public class OrderLine
{
    public string Sku { get; set; } = null!;

    public string? Description { get; set; }

    public int QuantityLine { get; set; }

    public decimal UnitPrice { get; set; }
}
