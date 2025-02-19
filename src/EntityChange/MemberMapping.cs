using EntityChange.Reflection;

namespace EntityChange;

/// <summary>
/// Mapping information for a class member.
/// </summary>
public class MemberMapping : IMemberOptions
{
    public MemberMapping(IMemberAccessor memberAccessor)
    {
        MemberAccessor = memberAccessor ?? throw new ArgumentNullException(nameof(memberAccessor));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the member is ignored.
    /// </summary>
    /// <value>
    ///   <c>true</c> if ignored; otherwise, <c>false</c>.
    /// </value>
    public bool Ignored { get; set; }

    /// <summary>
    /// Gets or sets the member accessor.
    /// </summary>
    /// <value>
    /// The member accessor.
    /// </value>
    public IMemberAccessor MemberAccessor { get; }


    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    /// <value>
    /// The display name.
    /// </value>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the equality <see langword="delegate" />.
    /// </summary>
    /// <value>
    /// The equality <see langword="delegate" />.
    /// </value>
    public Func<object?, object?, bool>? Equality { get; set; }

    /// <summary>
    /// Gets or sets the member value string formatter <see langword="delegate" />.
    /// </summary>
    /// <value>
    /// The string formatter <see langword="delegate" />.
    /// </value>
    public Func<object?, string?>? Formatter { get; set; }

    /// <summary>
    /// Gets or sets the collection comparison.
    /// </summary>
    /// <value>
    /// The collection comparison.
    /// </value>
    public CollectionComparison CollectionComparison { get; set; }
}
