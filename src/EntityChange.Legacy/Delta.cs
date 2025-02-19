namespace EntityChange;

/// <summary>
/// The delta result of two lists
/// </summary>
/// <typeparam name="TItem">The type of the item.</typeparam>
public class Delta<TItem>
{
    /// <summary>
    /// Gets or sets the list of items that matched and exist in both lists.
    /// </summary>
    /// <value>
    /// The list of items that matched.
    /// </value>
    public IEnumerable<TItem> Matched { get; set; }

    /// <summary>
    /// Gets or sets the list of new items created.
    /// </summary>
    /// <value>
    /// The list of new items created..
    /// </value>
    public IEnumerable<TItem> Created { get; set; }

    /// <summary>
    /// Gets or sets the list of items deleted.
    /// </summary>
    /// <value>
    /// The list of items deleted.
    /// </value>
    public IEnumerable<TItem> Deleted { get; set; }
}
