using System.Collections;
using System.Reflection;

using EntityChange.Extensions;
using EntityChange.Reflection;

namespace EntityChange;

/// <summary>
/// A class to compare two entities generating a change list.
/// </summary>
public class EntityComparer : IEntityComparer
{
    private readonly PathStack _pathStack;
    private readonly Stack<IMemberOptions> _memberStack;
    private readonly List<ChangeRecord> _changes;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityComparer"/> class.
    /// </summary>
    public EntityComparer() : this(EntityConfiguration.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityComparer"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public EntityComparer(IEntityConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _changes = [];
        _pathStack = new();
        _memberStack = [];
    }


    /// <summary>
    /// Gets the generator configuration.
    /// </summary>
    /// <value>
    /// The generator configuration.
    /// </value>
    public IEntityConfiguration Configuration { get; }


    /// <summary>
    /// Compares the specified <paramref name="original"/> and <paramref name="current"/> entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="original">The original entity.</param>
    /// <param name="current">The current entity.</param>
    /// <returns>A list of changes.</returns>
    public IReadOnlyList<ChangeRecord> Compare<TEntity>(TEntity? original, TEntity? current)
    {
        _changes.Clear();
        _pathStack.Clear();
        _memberStack.Clear();

        var type = typeof(TEntity);

        CompareType(type, original, current);

        return _changes;
    }


    private void CompareType(Type type, object? original, object? current, IMemberOptions? options = null)
    {
        // both null, nothing to compare
        if (original is null && current is null)
            return;

        if (type.IsArray)
            CompareArray(original, current, options);
        else if (original is IDictionary || current is IDictionary)
            CompareDictionary(original, current);
        else if (type.IsDictionary(out var keyType, out var elementType))
            CompareGenericDictionary(original, current, keyType, elementType);
        else if (original is IList || current is IList)
            CompareList(original, current, options);
        else if (type.IsCollection())
            CompareCollection(original, current, options);
        else if (type.GetTypeInfo().IsValueType || type == typeof(string))
            CompareValue(original, current, options);
        else
            CompareObject(type, original, current);
    }

    private void CompareObject(Type type, object? original, object? current)
    {
        // both null, nothing to compare
        if (original is null && current is null)
            return;

        if (original is null)
        {
            CreateChange(ChangeOperation.Replace, null, current);
            return;
        }

        if (current is null)
        {
            CreateChange(ChangeOperation.Replace, original, null);
            return;
        }

        var classMapping = Configuration.GetMapping(type);
        foreach (var memberMapping in classMapping.Members.Where(member => !member.Ignored))
        {
            var accessor = memberMapping.MemberAccessor;

            var originalValue = accessor.GetValue(original);
            var currentValue = accessor.GetValue(current);

            var propertyName = accessor.Name;

            // using value type to support abstracts
            var propertyType = originalValue?.GetType()
                ?? currentValue?.GetType()
                ?? accessor.MemberType.GetUnderlyingType();

            _memberStack.Push(memberMapping);
            _pathStack.PushProperty(propertyName);
            CompareType(propertyType, originalValue, currentValue, memberMapping);
            _pathStack.Pop();
            _memberStack.TryPop(out _);
        }
    }


    private void CompareDictionary(object? original, object? current)
    {
        var originalDictionary = original as IDictionary;
        var currentDictionary = current as IDictionary;

        // both null, nothing to compare
        if (originalDictionary is null && currentDictionary is null)
            return;

        CompareByKey(originalDictionary, currentDictionary, static d => d.Keys, static (d, k) => d[k]);
    }

    private void CompareGenericDictionary(object? original, object? current, Type keyType, Type elementType)
    {
        // both null, nothing to compare
        if (original is null && current is null)
            return;

        // TODO improve this, currently slow due to CreateInstance usage
        var t = typeof(DictionaryWrapper<,>).MakeGenericType(keyType, elementType);
        var o = Activator.CreateInstance(t, original) as IDictionaryWrapper;
        var c = Activator.CreateInstance(t, current) as IDictionaryWrapper;

        if (o is null && c is null)
            return;

        CompareByKey(o, c, static d => d.GetKeys(), static (d, k) => d.GetValue(k));
    }


    private void CompareArray(object? original, object? current, IMemberOptions? options)
    {
        var originalArray = original as Array;
        var currentArray = current as Array;

        // both null, nothing to compare
        if (originalArray is null && currentArray is null)
            return;

        if (options?.CollectionComparison == CollectionComparison.ObjectEquality)
            CompareByEquality(originalArray, currentArray, options);
        else
            CompareByIndexer(originalArray, currentArray, static t => t.Length, static (t, i) => t.GetValue(i));
    }

    private void CompareList(object? original, object? current, IMemberOptions? options)
    {
        var originalList = original as IList;
        var currentList = current as IList;

        // both null, nothing to compare
        if (originalList is null && currentList is null)
            return;

        if (options?.CollectionComparison == CollectionComparison.ObjectEquality)
            CompareByEquality(originalList, currentList, options);
        else
            CompareByIndexer(originalList, currentList, static t => t.Count, static (t, i) => t[i]);
    }

    private void CompareCollection(object? original, object? current, IMemberOptions? options)
    {
        var originalEnumerable = original as IEnumerable;
        var currentEnumerable = current as IEnumerable;

        // both null, nothing to compare
        if (originalEnumerable is null && currentEnumerable is null)
            return;

        if (options?.CollectionComparison == CollectionComparison.ObjectEquality)
        {
            CompareByEquality(originalEnumerable, currentEnumerable, options);
            return;
        }

        // convert to object array
        var originalArray = originalEnumerable?.Cast<object>().ToArray();
        var currentArray = currentEnumerable?.Cast<object>().ToArray();

        CompareByIndexer(originalArray, currentArray, static t => t.Length, static (t, i) => t.GetValue(i));
    }


    private void CompareValue(object? original, object? current, IMemberOptions? options)
    {
        var compare = options?.Equality ?? Equals;
        bool areEqual = compare(original, current);

        if (areEqual)
            return;

        CreateChange(ChangeOperation.Replace, original, current);
    }


    private void CompareByEquality(IEnumerable? original, IEnumerable? current, IMemberOptions? options)
    {
        var originalList = original?.Cast<object>().ToList() ?? [];
        var currentList = current?.Cast<object>().ToList() ?? [];

        var compare = options?.Equality ?? Equals;
        for (int index = 0; index < currentList.Count; index++)
        {
            var v = currentList[index];
            var o = originalList.FirstOrDefault(f => compare(f, v));

            if (o == null)
            {
                // added item
                CreateChange(ChangeOperation.Add, null, v);
                continue;
            }

            // remove so can't be reused
            originalList.Remove(o);

            var t = o.GetType();

            _pathStack.PushIndex(index);
            CompareType(t, o, v, options);
            _pathStack.Pop();
        }

        // removed items
        foreach (var v in originalList)
            CreateChange(ChangeOperation.Remove, v, null);
    }

    private void CompareByIndexer<T>(T? originalList, T? currentList, Func<T, int> countFactory, Func<T, int, object?> valueFactory)
    {
        if (countFactory == null)
            throw new ArgumentNullException(nameof(countFactory));
        if (valueFactory == null)
            throw new ArgumentNullException(nameof(valueFactory));

        var originalCount = originalList != null ? countFactory(originalList) : 0;
        var currentCount = currentList != null ? countFactory(currentList) : 0;

        int commonCount = Math.Min(originalCount, currentCount);

        // compare common items
        if (commonCount > 0)
        {
            for (int i = 0; i < commonCount; i++)
            {
                var o = originalList != null ? valueFactory(originalList, i) : null;
                var v = currentList != null ? valueFactory(currentList, i) : null;

                // skip nulls
                if (o is null && v is null)
                    continue;

                // get dictionary value type
                var t = o?.GetType() ?? v?.GetType();
                if (t is null)
                    continue;

                _pathStack.PushIndex(i);
                CompareType(t, o, v);
                _pathStack.Pop();
            }
        }

        // added items
        if (commonCount < currentCount)
        {
            for (int i = commonCount; i < currentCount; i++)
            {
                var v = currentList != null ? valueFactory(currentList, i) : null;

                _pathStack.PushIndex(i);
                CreateChange(ChangeOperation.Add, null, v);
                _pathStack.Pop();
            }
        }

        // removed items
        if (commonCount < originalCount)
        {
            for (int i = commonCount; i < originalCount; i++)
            {
                var v = originalList != null ? valueFactory(originalList, i) : null;

                _pathStack.PushIndex(i);
                CreateChange(ChangeOperation.Remove, v, null);
                _pathStack.Pop();
            }
        }
    }

    private void CompareByKey<T>(T? originalDictionary, T? currentDictionary, Func<T, IEnumerable> keysFactory, Func<T, object, object?> valueFactory)
    {
        if (keysFactory == null)
            throw new ArgumentNullException(nameof(keysFactory));
        if (valueFactory == null)
            throw new ArgumentNullException(nameof(valueFactory));

        List<object> originalKeys = originalDictionary != null ? [.. keysFactory(originalDictionary).Cast<object>()] : [];
        List<object> currentKeys = currentDictionary != null ? [.. keysFactory(currentDictionary).Cast<object>()] : [];

        // compare common keys
        var commonKeys = originalKeys.Intersect(currentKeys).ToList();
        foreach (var key in commonKeys)
        {
            // safe to use indexer because keys are common
            var o = originalDictionary != null ? valueFactory(originalDictionary, key) : null;
            var v = currentDictionary != null ? valueFactory(currentDictionary, key) : null;

            // skip nulls
            if (o is null && v is null)
                continue;

            // get dictionary value type
            var t = o?.GetType() ?? v?.GetType();
            if (t is null)
                continue;

            _pathStack.PushKey(key);
            CompareType(t, o, v);
            _pathStack.Pop();
        }

        // new key changes
        var addedKeys = currentKeys.Except(originalKeys).ToList();
        foreach (var key in addedKeys)
        {
            var v = currentDictionary != null ? valueFactory(currentDictionary, key) : null;

            _pathStack.PushKey(key);
            CreateChange(ChangeOperation.Add, null, v);
            _pathStack.Pop();
        }

        // removed key changes
        var removedKeys = originalKeys.Except(currentKeys).ToList();
        foreach (var key in removedKeys)
        {
            var v = originalDictionary != null ? valueFactory(originalDictionary, key) : null;

            _pathStack.PushKey(key);
            CreateChange(ChangeOperation.Remove, v, null);
            _pathStack.Pop();
        }
    }


    private IMemberOptions? CurrentMember()
    {
        return _memberStack.Count > 0
            ? _memberStack.Peek()
            : null;
    }

    private void CreateChange(
        ChangeOperation operation,
        object? original,
        object? current)
    {
        var currentMember = CurrentMember();
        var propertyName = _pathStack.CurrentName();
        var displayName = currentMember?.DisplayName ?? propertyName.ToTitle();
        var currentPath = _pathStack.ToString();
        var originalFormatted = FormatValue(original, currentMember?.Formatter);
        var currentFormatted = FormatValue(current, currentMember?.Formatter);

        var changeRecord = new ChangeRecord
        {
            PropertyName = propertyName,
            DisplayName = displayName,
            Path = currentPath,
            Operation = operation,
            OriginalValue = original,
            CurrentValue = current,
            OriginalFormatted = originalFormatted,
            CurrentFormatted = currentFormatted,
        };

        _changes.Add(changeRecord);
    }

    private static string? FormatValue(object? value, Func<object?, string?>? formatter)
    {
        if (value is null)
            return null;

        if (formatter is not null)
            return formatter(value);

        return value?.ToString();
    }
}
