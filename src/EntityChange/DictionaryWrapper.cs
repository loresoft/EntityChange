using System.Collections;

namespace EntityChange;

/// <summary>
/// Generic dictionary wrapper to allow comparison by object type
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
internal class DictionaryWrapper<TKey, TValue> : IDictionaryWrapper
{
    private readonly IDictionary<TKey, TValue> _dictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryWrapper{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
    {
        _dictionary = dictionary;
    }


    /// <summary>
    /// Gets the underling dictionary keys.
    /// </summary>
    /// <returns>An <see cref="IEnumerable"/> of keys</returns>
    public IEnumerable GetKeys()
    {
        return _dictionary.Keys;
    }


    /// <summary>
    /// Gets the value for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The dictionary key.</param>
    /// <returns>The value for the specified <paramref name="key"/></returns>
    public object? GetValue(object key)
    {
        _dictionary.TryGetValue((TKey)key, out var value);

        return value;
    }
}
