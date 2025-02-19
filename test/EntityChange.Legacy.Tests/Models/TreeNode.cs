using System.Collections.Generic;

namespace EntityChange.Tests.Models;

public class TreeNode
{
    public List<TreeNode> nodes { get; set; }
    public string Name { get; set; }
}
