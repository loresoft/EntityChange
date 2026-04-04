using System.Collections;

namespace EntityChange;

/// <summary>
/// Abstract base class for source-generated entity comparers.
/// Provides shared comparison helpers used by generated code.
/// </summary>
/// <typeparam name="T">The type of entity to compare.</typeparam>
public abstract class EntityComparer<T> : IEntityComparer<T>
{
    /// <summary>
    /// Gets the path stack used to track object graph navigation.
    /// </summary>
    protected PathStack PathStack { get; } = new();

    /// <summary>
    /// Gets the list of accumulated changes.
    /// </summary>
    protected List<ChangeRecord> Changes { get; } = [];

    /// <summary>
    /// Compares the specified <paramref name="original"/> and <paramref name="current"/> entities.
    /// </summary>
    /// <param name="original">The original entity.</param>
    /// <param name="current">The current entity.</param>
    /// <returns>A list of changes.</returns>
    public abstract IReadOnlyList<ChangeRecord> Compare(T? original, T? current);

    /// <inheritdoc/>
    IReadOnlyList<ChangeRecord> IEntityComparer.Compare<TEntity>(TEntity? original, TEntity? current)
        where TEntity : default
    {
        if (original is T o)
        {
            if (current is T c)
                return Compare(o, c);

            return Compare(o, default);
        }

        if (current is T c2)
            return Compare(default, c2);

        return Compare(default, default);
    }

    /// <summary>
    /// Records a change in the change list.
    /// </summary>
    protected void CreateChange(
        ChangeOperation operation,
        string propertyName,
        string displayName,
        object? original,
        object? current,
        Func<object?, string?>? formatter = null)
    {
        var currentPath = PathStack.ToString();
        var originalFormatted = FormatValue(original, formatter);
        var currentFormatted = FormatValue(current, formatter);

        Changes.Add(new ChangeRecord
        {
            PropertyName = propertyName,
            DisplayName = displayName,
            Path = currentPath,
            Operation = operation,
            OriginalValue = original,
            CurrentValue = current,
            OriginalFormatted = originalFormatted,
            CurrentFormatted = currentFormatted,
        });
    }

    /// <summary>
    /// Compares two values of a specific type. Generic to avoid boxing for value types.
    /// </summary>
    protected void CompareValue<TValue>(
        TValue? original,
        TValue? current,
        string propertyName,
        string displayName,
        Func<object?, string?>? formatter = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(original!, current!))
            return;

        CreateChange(ChangeOperation.Replace, propertyName, displayName, original, current, formatter);
    }

    /// <summary>
    /// Compares two collections by index position.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <param name="original">The original collection.</param>
    /// <param name="current">The current collection.</param>
    /// <param name="deepCompare">Optional callback to deeply compare complex elements.</param>
    /// <param name="formatter">Optional formatter for element values.</param>
    protected void CompareListByIndex<TElement>(
        IEnumerable<TElement>? original,
        IEnumerable<TElement>? current,
        Action<TElement?, TElement?>? deepCompare = null,
        Func<object?, string?>? formatter = null)
    {
        var originalList = original != null ? new List<TElement>(original) : null;
        var currentList = current != null ? new List<TElement>(current) : null;

        if (originalList is null && currentList is null)
            return;

        var originalCount = originalList?.Count ?? 0;
        var currentCount = currentList?.Count ?? 0;
        var commonCount = Math.Min(originalCount, currentCount);

        // compare common items
        for (int i = 0; i < commonCount; i++)
        {
            var o = originalList![i];
            var c = currentList![i];

            if (o is null && c is null)
                continue;

            PathStack.PushIndex(i);

            if (deepCompare != null)
                deepCompare(o, c);
            else
                CompareElementValue(o, c, formatter);

            PathStack.Pop();
        }

        // added items
        for (int i = commonCount; i < currentCount; i++)
        {
            var v = currentList![i];
            PathStack.PushIndex(i);
            CreateChange(ChangeOperation.Add, PathStack.CurrentName(), PathStack.CurrentName(), null, v, formatter);
            PathStack.Pop();
        }

        // removed items
        for (int i = commonCount; i < originalCount; i++)
        {
            var v = originalList![i];
            PathStack.PushIndex(i);
            CreateChange(ChangeOperation.Remove, PathStack.CurrentName(), PathStack.CurrentName(), v, null, formatter);
            PathStack.Pop();
        }
    }

    /// <summary>
    /// Compares two sets using set difference semantics.
    /// </summary>
    protected void CompareSet<TElement>(
        IEnumerable<TElement>? original,
        IEnumerable<TElement>? current,
        Func<object?, string?>? formatter = null)
    {
        var originalSet = original != null ? new HashSet<TElement>(original) : [];
        var currentSet = current != null ? new HashSet<TElement>(current) : [];

        if (originalSet.Count == 0 && currentSet.Count == 0)
            return;

        // added items (in current but not original)
        var index = 0;
        foreach (var item in currentSet)
        {
            if (!originalSet.Contains(item))
            {
                PathStack.PushIndex(index);
                CreateChange(ChangeOperation.Add, PathStack.CurrentName(), PathStack.CurrentName(), null, item, formatter);
                PathStack.Pop();
            }
            index++;
        }

        // removed items (in original but not current)
        index = 0;
        foreach (var item in originalSet)
        {
            if (!currentSet.Contains(item))
            {
                PathStack.PushIndex(index);
                CreateChange(ChangeOperation.Remove, PathStack.CurrentName(), PathStack.CurrentName(), item, null, formatter);
                PathStack.Pop();
            }
            index++;
        }
    }

    /// <summary>
    /// Compares two generic dictionaries by key.
    /// </summary>
    protected void CompareDictionary<TKey, TValue>(
        IDictionary<TKey, TValue>? original,
        IDictionary<TKey, TValue>? current,
        Action<TValue?, TValue?>? deepCompare = null)
        where TKey : notnull
    {
        if (original is null && current is null)
            return;

        var originalKeys = original?.Keys.ToList() ?? [];
        var currentKeys = current?.Keys.ToList() ?? [];

        // common keys
        var commonKeys = originalKeys.Intersect(currentKeys).ToList();
        foreach (var key in commonKeys)
        {
            var o = original != null ? original[key] : default;
            var v = current != null ? current[key] : default;

            if (o is null && v is null)
                continue;

            PathStack.PushKey(key);

            if (deepCompare != null)
                deepCompare(o, v);
            else
                CompareElementValue(o, v);

            PathStack.Pop();
        }

        // added keys
        var addedKeys = currentKeys.Except(originalKeys).ToList();
        foreach (var key in addedKeys)
        {
            var v = current != null ? current[key] : default;
            PathStack.PushKey(key);
            CreateChange(ChangeOperation.Add, PathStack.CurrentName(), PathStack.CurrentName(), null, v);
            PathStack.Pop();
        }

        // removed keys
        var removedKeys = originalKeys.Except(currentKeys).ToList();
        foreach (var key in removedKeys)
        {
            var v = original != null ? original[key] : default;
            PathStack.PushKey(key);
            CreateChange(ChangeOperation.Remove, PathStack.CurrentName(), PathStack.CurrentName(), v, null);
            PathStack.Pop();
        }
    }

    /// <summary>
    /// Compares two non-generic dictionaries by key.
    /// </summary>
    protected void CompareDictionary(IDictionary? original, IDictionary? current)
    {
        if (original is null && current is null)
            return;

        var originalKeys = original != null ? original.Keys.Cast<object>().ToList() : [];
        var currentKeys = current != null ? current.Keys.Cast<object>().ToList() : [];

        // common keys
        var commonKeys = originalKeys.Intersect(currentKeys).ToList();
        foreach (var key in commonKeys)
        {
            var o = original?[key];
            var v = current?[key];

            if (o is null && v is null)
                continue;

            PathStack.PushKey(key);
            CompareElementValue(o, v);
            PathStack.Pop();
        }

        // added keys
        var addedKeys = currentKeys.Except(originalKeys).ToList();
        foreach (var key in addedKeys)
        {
            var v = current?[key];
            PathStack.PushKey(key);
            CreateChange(ChangeOperation.Add, PathStack.CurrentName(), PathStack.CurrentName(), null, v);
            PathStack.Pop();
        }

        // removed keys
        var removedKeys = originalKeys.Except(currentKeys).ToList();
        foreach (var key in removedKeys)
        {
            var v = original?[key];
            PathStack.PushKey(key);
            CreateChange(ChangeOperation.Remove, PathStack.CurrentName(), PathStack.CurrentName(), v, null);
            PathStack.Pop();
        }
    }

    /// <summary>
    /// Compares a polymorphic (abstract/interface) property using Equals().
    /// Used when the concrete type is unknown at generation time.
    /// </summary>
    protected void ComparePolymorphic(
        object? original,
        object? current,
        string propertyName,
        string displayName)
    {
        if (original is null && current is null)
            return;

        if (Equals(original, current))
            return;

        CreateChange(ChangeOperation.Replace, propertyName, displayName, original, current);
    }

    private void CompareElementValue<TValue>(TValue? original, TValue? current, Func<object?, string?>? formatter = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(original!, current!))
            return;

        var name = PathStack.CurrentName();
        CreateChange(ChangeOperation.Replace, name, name, original, current, formatter);
    }

    private static string? FormatValue(object? value, Func<object?, string?>? formatter)
    {
        if (value is null)
            return null;

        if (formatter is not null)
            return formatter(value);

        return value.ToString();
    }
}
