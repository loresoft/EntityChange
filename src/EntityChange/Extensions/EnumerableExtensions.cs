namespace EntityChange.Extensions;

/// <summary>
/// Extension methods for <see cref="IEnumerable{T}"/>
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts an IEnumerable of values to a delimited string.
    /// </summary>
    /// <typeparam name="T">
    /// The type of objects to delimit.
    /// </typeparam>
    /// <param name="values">
    /// The IEnumerable string values to convert.
    /// </param>
    /// <param name="delimiter">
    /// The delimiter.
    /// </param>
    /// <returns>
    /// A delimited string of the values.
    /// </returns>
    public static string ToDelimitedString<T>(this IEnumerable<T?> values, string? delimiter = ",")
        => string.Join(delimiter ?? ",", values);

    /// <summary>
    /// Converts an IEnumerable of values to a delimited string.
    /// </summary>
    /// <param name="values">The IEnumerable string values to convert.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns>A delimited string of the values.</returns>
    public static string ToDelimitedString(this IEnumerable<string?> values, string? delimiter = ",")
        => string.Join(delimiter ?? ",", values);

    /// <summary>
    /// Compares the specified existing and current lists returning the delta between them.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="existing">The existing list of items.</param>
    /// <param name="current">The new and current list of items.</param>
    /// <param name="comparer">The comparer the <see cref="IEqualityComparer{T}"/> used to compare the items.</param>
    /// <returns>The <see cref="Delta{TItem}"/> result of two lists.</returns>
    /// <exception cref="ArgumentNullException">when <paramref name="existing"/> or <paramref name="current"/> lists are null.</exception>
    public static Delta<TItem> DeltaCompare<TItem>(this IEnumerable<TItem> existing, IEnumerable<TItem> current, IEqualityComparer<TItem>? comparer = null)
    {
        if (existing is null)
            throw new ArgumentNullException(nameof(existing));

        if (current is null)
            throw new ArgumentNullException(nameof(current));

        comparer ??= EqualityComparer<TItem>.Default;

        var existingList = existing.ToList();
        var currentList = current.ToList();

        var matched = existingList.Intersect(currentList, comparer);
        var created = currentList.Except(existingList, comparer);
        var deleted = existingList.Except(currentList, comparer);

        return new Delta<TItem>
        {
            Created = created,
            Deleted = deleted,
            Matched = matched
        };
    }
}
