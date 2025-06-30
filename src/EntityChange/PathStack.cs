using System.Collections.Concurrent;
using System.IO;
using System.Text.Json.Serialization;

using EntityChange.Extensions;

namespace EntityChange;

/// <summary>
/// Path stack structure used to compute object graph paths expressions
/// </summary>
public class PathStack
{
    private readonly ConcurrentStack<PathValue> _pathStack = [];

    /// <summary>
    /// Push a property name to the stack
    /// </summary>
    /// <param name="propertyName">The name of the property</param>
    public void PushProperty(string propertyName)
        => _pathStack.Push(new(propertyName ?? string.Empty, Separator: '.'));

    /// <summary>
    /// Push an indexer to the stack. Will be converted to string.
    /// </summary>
    /// <typeparam name="T">The type of the indexer</typeparam>
    /// <param name="index">The indexer value. Will be converted to string</param>
    public void PushIndex<T>(T index)
        => _pathStack.Push(new(index?.ToString() ?? string.Empty, Indexer: true));

    /// <summary>
    /// Push a key indexer to the stack. Will be converted to string.
    /// </summary>
    /// <typeparam name="T">The type of the key indexer</typeparam>
    /// <param name="key">The key indexer value. Will be converted to string.</param>
    public void PushKey<T>(T key)
        => _pathStack.Push(new(key?.ToString() ?? string.Empty, Indexer: true));

    /// <summary>
    /// Pop the last path off the stack
    /// </summary>
    public void Pop()
        => _pathStack.TryPop(out _);

    /// <summary>
    /// Clear the path stack
    /// </summary>
    public void Clear()
        => _pathStack.Clear();

    /// <summary>
    /// Gets the top path name from the stack.
    /// </summary>
    /// <returns>The top path name</returns>
    public string CurrentName()
    {
        if (_pathStack.IsEmpty)
            return string.Empty;

        if (!_pathStack.TryPeek(out var peeked))
            return string.Empty;

        // not an indexer, use as is
        if (peeked.Indexer != true)
            return peeked.Name;

        // only item, use indexer path
        if (_pathStack.Count == 1)
            return $"[{peeked.Name}]";

        // add indexers till property is reached
        var paths = new List<PathValue>();
        var pathList = _pathStack.ToList();

        for (int i = 0; i < pathList.Count; i++)
        {
            var path = pathList[i];
            paths.Add(path);
            if (path.Indexer != true)
                break;
        }

        // create path expression
        return ToPath([.. paths]);
    }

    /// <summary>
    /// Gets the top property name from the stack
    /// </summary>
    /// <returns>The top property name</returns>
    public string CurrentProperty()
    {
        if (_pathStack.Count == 0)
            return string.Empty;

        // find first none indexer path
        var lastProperty = _pathStack.FirstOrDefault(p => p.Indexer != true);
        return lastProperty.Name ?? string.Empty;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var array = _pathStack.ToArray();
        return ToPath(array);
    }


    private static string ToPath(PathValue[] values)
    {
        var sb = StringBuilderCache.Acquire();

        // stack is in reverse order
        for (int i = values.Length - 1; i >= 0; i--)
        {
            var value = values[i];

            if (sb.Length > 0 && value.Separator.HasValue)
                sb.Append(value.Separator);

            if (value.Indexer == true)
                sb.Append('[').Append(value.Name).Append(']');
            else
                sb.Append(value.Name);
        }

        return StringBuilderCache.ToString(sb);
    }

    readonly record struct PathValue(string Name, char? Separator = null, bool? Indexer = false);
}
