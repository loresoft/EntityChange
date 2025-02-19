using System.Collections.Generic;

using EntityChange.Attributes;

namespace EntityChange.Tests.Models;

public class Order
{
    public string Id { get; set; } = null!;

    public int OrderNumber { get; set; }

    public Contact? Contact { get; set; }

    public MailingAddress ShippingAddress { get; set; } = new();

    public MailingAddress BillingAddress { get; set; } = new();

    public List<OrderLine> Items { get; set; } = [];

    public decimal Total { get; set; }

    public string? Ignored { get; set; }
}
