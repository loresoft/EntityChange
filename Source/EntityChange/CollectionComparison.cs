using System;

namespace EntityChange
{
    /// <summary>
    /// The type of collection comparison.
    /// </summary>
    public enum CollectionComparison
    {
        /// <summary>
        /// Compare collection by index position.
        /// </summary>
        CollectionIndexer,
        /// <summary>
        /// Compare collection by element equality.
        /// </summary>
        ObjectEquality
    }
}