#nullable disable

namespace EntityChange;

/// <summary>
/// Defines methods to support the comparison of objects key for equality.
/// </summary>
/// <typeparam name="TComparer">The type of the comparer.</typeparam>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <seealso cref="IEqualityComparer{TComparer}" />
public class KeyEqualityComparer<TComparer, TKey> : IEqualityComparer<TComparer>
{
    private readonly Func<TComparer, TKey> _keySelector;
    private readonly IEqualityComparer<TKey> _comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyEqualityComparer{TComparer, TKey}"/> class.
    /// </summary>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="comparer">The comparer.</param>
    /// <exception cref="ArgumentNullException">keySelector</exception>
    public KeyEqualityComparer(Func<TComparer, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
    }

    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    ///   <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(TComparer x, TComparer y)
    {
        return _comparer.Equals(_keySelector(x), _keySelector(y));
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public int GetHashCode(TComparer obj)
    {
        return _comparer.GetHashCode(_keySelector(obj));
    }

    /// <summary>
    /// Creates an <see cref="KeyEqualityComparer{TComparer, TKey}"/> from the specified key selector expression.
    /// </summary>
    /// <param name="keySelector">The key selector.</param>
    /// <returns></returns>
    public static IEqualityComparer<TComparer> Create(Func<TComparer, TKey> keySelector)
    {
        return Create(keySelector, EqualityComparer<TKey>.Default);
    }

    /// <summary>
    /// Creates an <see cref="KeyEqualityComparer{TComparer, TKey}"/> from the specified key selector expression.
    /// </summary>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="comparer">The key comparer.</param>
    /// <returns></returns>
    public static IEqualityComparer<TComparer> Create(Func<TComparer, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        return new KeyEqualityComparer<TComparer, TKey>(keySelector, comparer);
    }
}
