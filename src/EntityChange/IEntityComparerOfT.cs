namespace EntityChange;

/// <summary>
/// A strongly-typed interface to compare two entities of type <typeparamref name="T"/> generating a change list.
/// </summary>
/// <typeparam name="T">The type of entity to compare.</typeparam>
public interface IEntityComparer<T> : IEntityComparer
{
    /// <summary>
    /// Compares the specified <paramref name="original"/> and <paramref name="current"/> entities.
    /// </summary>
    /// <param name="original">The original entity.</param>
    /// <param name="current">The current entity.</param>
    /// <returns>A list of changes.</returns>
    IReadOnlyList<ChangeRecord> Compare(T? original, T? current);
}
