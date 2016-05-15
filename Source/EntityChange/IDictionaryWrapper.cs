using System;
using System.Collections;

namespace EntityChange
{
    /// <summary>
    /// Generic dictionary wrapper <see langword="interface"/> to allow comparison by object type
    /// </summary>
    internal interface IDictionaryWrapper
    {
        /// <summary>
        /// Gets the underling dictionary keys.
        /// </summary>
        /// <returns>An <see cref="IEnumerable"/> of keys</returns>
        IEnumerable GetKeys();
        
        /// <summary>
        /// Gets the value for the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The dictionary key.</param>
        /// <returns>The value for the specified <paramref name="key"/></returns>
        object GetValue(object key);
    }
}