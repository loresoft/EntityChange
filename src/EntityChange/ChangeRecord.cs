using System.Text.Json.Serialization;

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
    [JsonPropertyName("propertyName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the display name for the changed property.
    /// </summary>
    /// <value>
    /// The display name for the changed property.
    /// </value>
    [JsonPropertyName("displayName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the object graph change path.
    /// </summary>
    /// <value>
    /// The object graph change path.
    /// </value>
    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the type of change operation.
    /// </summary>
    /// <value>
    /// The type of change operation.
    /// </value>
    [JsonPropertyName("operation")]
    [JsonConverter(typeof(JsonStringEnumConverter<ChangeOperation>))]
    public ChangeOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the original value.
    /// </summary>
    /// <value>
    /// The original value.
    /// </value>
    [JsonPropertyName("originalValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? OriginalValue { get; set; }

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    /// <value>
    /// The current value.
    /// </value>
    [JsonPropertyName("currentValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? CurrentValue { get; set; }

    /// <summary>
    /// Gets or sets the original value formatted as a <see cref="string"/>.
    /// </summary>
    /// <value>
    /// The original value formatted as a <see cref="string"/>.
    /// </value>
    [JsonPropertyName("originalFormatted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OriginalFormatted { get; set; }

    /// <summary>
    /// Gets or sets the current value formatted as a <see cref="string"/>.
    /// </summary>
    /// <value>
    /// The current value formatted as a <see cref="string"/>.
    /// </value>
    [JsonPropertyName("currentFormatted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrentFormatted { get; set; }
}
