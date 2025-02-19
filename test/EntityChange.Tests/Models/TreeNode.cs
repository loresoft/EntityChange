using System.Collections.Generic;

using EntityChange.Attributes;

namespace EntityChange.Tests.Models;

public class TreeNode
{
    public List<TreeNode>? Nodes { get; set; }

    public string? Name { get; set; }
}
