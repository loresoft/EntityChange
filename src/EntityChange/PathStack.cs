using System.Text;

using EntityChange.Extensions;

namespace EntityChange;

public class PathStack
{
    private readonly Stack<PathValue> _pathStack = [];

    public void PushProperty(string propertyName)
        => _pathStack.Push(new(propertyName, Separator: '.'));

    public void PushIndex<T>(T index)
        => _pathStack.Push(new(index?.ToString() ?? string.Empty, Indexer: true));

    public void PushKey<T>(T key)
        => _pathStack.Push(new(key?.ToString() ?? string.Empty, Indexer: true));

    public void Pop()
        => _pathStack.TryPop(out _);

    public void Clear()
        => _pathStack.Clear();

    public string CurrentName()
    {
        if (_pathStack.Count == 0)
            return string.Empty;

        var peeked = _pathStack.Peek();

        // not an indexer, use as is
        if (peeked.Indexer != true)
            return peeked.Value;

        // only item, use indexer path
        if (_pathStack.Count == 1)
            return $"[{peeked.Value}]";

        // get top 2 items to create path name
        return ToPath([.. _pathStack.Take(2)]);
    }

    public string CurrentProperty()
    {
        if (_pathStack.Count == 0)
            return string.Empty;

        // find first none indexer path
        var lastProperty = _pathStack.FirstOrDefault(p => p.Indexer != true);
        return lastProperty.Value ?? string.Empty;
    }

    public override string ToString()
    {
        return ToPath([.. _pathStack]);
    }

    private static string ToPath(PathValue[] values)
    {
        var sb = StringBuilderCache.Acquire();
        for (int i = values.Length - 1; i >= 0; i--)
        {
            var value = values[i];

            if (sb.Length > 0 && value.Separator.HasValue)
                sb.Append(value.Separator);

            if (value.Indexer == true)
                sb.Append('[').Append(value.Value).Append(']');
            else
                sb.Append(value.Value);
        }

        return StringBuilderCache.ToString(sb);
    }

    record struct PathValue(string Value, char? Separator = null, bool? Indexer = false);
}
