using System.Collections.Generic;

namespace EntityChange;

/// <summary>
/// An <see langword="interface"/> for detect changes between to instances
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IChangeDetector<TEntity> where TEntity : class
{
    /// <summary>
    /// Detect changes with the specified <paramref name="original"/> and <paramref name="current"/> entities.
    /// </summary>
    /// <param name="original">The original entity.</param>
    /// <param name="current">The current entity.</param>
    /// <returns>A list of changes.</returns>
    IReadOnlyList<ChangeRecord> DetectChanges(TEntity? original, TEntity? current);
}

public abstract class ChangeDetector<TEntity> : IChangeDetector<TEntity>
    where TEntity : class
{

    public abstract IReadOnlyList<ChangeRecord> DetectChanges(TEntity? original, TEntity? current);

    protected ChangeRecord? ComparePropertyValue<TValue>(
        TValue originalValue,
        TValue currentValue,
        string propertyName,
        string displayName,
        string? formatString = null,
        string? parentPath = null,
        IEqualityComparer<TValue>? comparer = null
    )
    {
        comparer ??= EqualityComparer<TValue>.Default;

        if (comparer.Equals(originalValue, currentValue))
            return null;

        return new ChangeRecord
        {
            PropertyName = propertyName,
            DisplayName = displayName,
            Operation = ChangeOperation.Replace,
            OriginalValue = originalValue,
            CurrentValue = currentValue,
            Path = PathCombine(parentPath, propertyName),
            OriginalFormatted = FormatValue(originalValue, formatString),
            CurrentFormatted = FormatValue(currentValue, formatString)
        };
    }

    protected string? FormatValue<T>(T? value, string? format)
    {
        if (value is null)
            return string.Empty;

        return string.IsNullOrEmpty(format)
          ? value.ToString()
          : string.Format("{0:" + format + "}", value);
    }

    protected string? PathCombine(string? parentPath, string? propertyName)
    {
        if (string.IsNullOrEmpty(parentPath))
            return propertyName;

        if (string.IsNullOrEmpty(propertyName))
            return parentPath;

        return $"{parentPath}.{propertyName}";
    }
}
