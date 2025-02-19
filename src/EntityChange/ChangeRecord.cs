namespace EntityChange;

/// <summary>
/// An entity change record
/// </summary>
public class ChangeRecord
{
    /// <summary>
    /// Gets or sets the name of the property that was changed.
    /// </summary>
    /// <value>
    /// The name of the property that was changed.
    /// </value>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the display name for the changed property.
    /// </summary>
    /// <value>
    /// The display name for the changed property.
    /// </value>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the object graph change path.
    /// </summary>
    /// <value>
    /// The object graph change path.
    /// </value>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the type of change operation.
    /// </summary>
    /// <value>
    /// The type of change operation.
    /// </value>
    public ChangeOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the original value.
    /// </summary>
    /// <value>
    /// The original value.
    /// </value>
    public object? OriginalValue { get; set; }

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    /// <value>
    /// The current value.
    /// </value>
    public object? CurrentValue { get; set; }

    /// <summary>
    /// Gets or sets the original value formatted as a <see cref="string"/>.
    /// </summary>
    /// <value>
    /// The original value formatted as a <see cref="string"/>.
    /// </value>
    public string? OriginalFormatted { get; set; }

    /// <summary>
    /// Gets or sets the current value formatted as a <see cref="string"/>.
    /// </summary>
    /// <value>
    /// The current value formatted as a <see cref="string"/>.
    /// </value>
    public string? CurrentFormatted { get; set; }
}
