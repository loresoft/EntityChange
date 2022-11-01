namespace EntityChange;

/// <summary>
/// An <see langword="interface"/> for member options
/// </summary>
public interface IMemberOptions
{
    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    /// <value>
    /// The display name.
    /// </value>
    string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the equality <see langword="delegate"/>.
    /// </summary>
    /// <value>
    /// The equality <see langword="delegate"/>.
    /// </value>
    Func<object, object, bool> Equality { get; set; }

    /// <summary>
    /// Gets or sets the string formatter <see langword="delegate" />.
    /// </summary>
    /// <value>
    /// The string formatter <see langword="delegate" />.
    /// </value>
    Func<object, string> Formatter { get; set; }

    /// <summary>
    /// Gets or sets the collection comparison.
    /// </summary>
    /// <value>
    /// The collection comparison.
    /// </value>
    CollectionComparison CollectionComparison { get; set; }
}