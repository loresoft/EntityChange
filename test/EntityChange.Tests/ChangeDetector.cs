using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EntityChange.Attributes;
using EntityChange.Tests.Models;

namespace EntityChange.Tests;


[ChangeDetector]
public partial class ChangeDetector
{
    public partial IReadOnlyList<ChangeRecord> DetectChanges(OrderLine? original, OrderLine? current);
}
