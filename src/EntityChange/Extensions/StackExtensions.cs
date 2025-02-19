using System.Diagnostics.CodeAnalysis;

namespace EntityChange.Extensions;

/// <summary>
/// Extension methods for <see cref="Stack{T}"/>
/// </summary>
public static class StackExtensions
{
#if NETSTANDARD2_0
    public static bool TryPop<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T result)
    {
        if (stack.Count == 0)
        {
            result = default;
            return false;
        }

        result = stack.Pop();
        return true;
    }

    public static bool TryPeek<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T result)
    {
        if (stack.Count == 0)
        {
            result = default;
            return false;
        }

        result = stack.Peek();
        return true;
    }
#endif
}
