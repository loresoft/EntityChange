using System;
using System.Collections.Generic;

using EntityChange.Attributes;

namespace EntityChange.Tests.Models;

public class Contact
{
    public string Id { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public string? JobTitle { get; set; }

    public Status Status { get; set; }

    public bool IsActive { get; set; }

    public string[]? Roles { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }

    public List<MailingAddress> MailingAddresses { get; set; } = [];

    public List<EmailAddress> EmailAddresses { get; set; } = [];

    public List<PhoneNumber> PhoneNumbers { get; set; } = [];

    public HashSet<string> Categories { get; set; } = [];

    public Dictionary<string, object>? Data { get; set; }
}
