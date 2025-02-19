using System.Text;

namespace EntityChange.Extensions;

/// <summary>
/// Extension methods for <see cref="IEnumerable{T}"/>
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts an IEnumerable of values to a delimited string.
    /// </summary>
    /// <typeparam name="T">The type of objects to delimit.</typeparam>
    /// <param name="values">The IEnumerable of values to convert.</param>
    /// <param name="delimiter">The string delimiter.</param>
    /// <param name="escapeDelimiter">A delegate used to escape the delimiter contained in the value.</param>
    /// <returns>
    /// A delimited string of the values.
    /// </returns>
    public static string ToDelimitedString<T>(this IEnumerable<T> values, string delimiter = ",", Func<string, string> escapeDelimiter = null)
    {
        var sb = new StringBuilder();
        foreach (var value in values)
        {
            if (sb.Length > 0)
                sb.Append(delimiter);

            var v = value?.ToString() ?? string.Empty;
            if (escapeDelimiter != null)
                v = escapeDelimiter(v);

            sb.Append(v);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Compares the specified existing and current lists returning the delta between them.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="existing">The existing list of items.</param>
    /// <param name="current">The new and current list of items.</param>
    /// <param name="comparer">The comparer the <see cref="IEqualityComparer{T}"/> used to compare the items.</param>
    /// <returns>The <see cref="Delta{TItem}"/> result of two lists.</returns>
    /// <exception cref="ArgumentNullException">when <paramref name="existing"/> or <paramref name="current"/> lists are null.</exception>
    public static Delta<TItem> DeltaCompare<TItem>(this IEnumerable<TItem> existing, IEnumerable<TItem> current, IEqualityComparer<TItem> comparer = null)
    {
        if (existing == null)
            throw new ArgumentNullException(nameof(existing));

        if (current == null)
            throw new ArgumentNullException(nameof(current));

        if (comparer == null)
            comparer = EqualityComparer<TItem>.Default;

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

