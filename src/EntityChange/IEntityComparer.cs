using System;
using System.Collections.ObjectModel;

namespace EntityChange
{
    /// <summary>
    /// A interface to compare two entities generating a change list. 
    /// </summary>
    public interface IEntityComparer
    {
        /// <summary>
        /// Compares the specified <paramref name="original"/> and <paramref name="current"/> entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="original">The original entity.</param>
        /// <param name="current">The current entity.</param>
        /// <returns>A list of changes.</returns>
        ReadOnlyCollection<ChangeRecord> Compare<TEntity>(TEntity original, TEntity current);
    }
}