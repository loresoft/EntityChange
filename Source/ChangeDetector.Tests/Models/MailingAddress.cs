using System;

namespace ChangeDetector.Tests.Models
{
    public class MailingAddress
    {
        public ContactType Type { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

    }
}