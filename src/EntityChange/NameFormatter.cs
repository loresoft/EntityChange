using System.Reflection;
using System.Text;

namespace EntityChange;

/// <summary>
/// Named string formatter.
/// </summary>
public static class NameFormatter
{
    /// <summary>
    /// Replaces each named format item in a specified string with the text equivalent of a corresponding object's property value.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="source">The object to format.</param>
    /// <returns>A copy of format in which any named format items are replaced by the string representation.</returns>
    /// <example>
    /// <code>
    /// var o = new { First = "John", Last = "Doe" };
    /// string result = NameFormatter.FormatName("Full Name: {First} {Last}", o);
    /// </code>
    /// </example>
    public static string FormatName(this string format, object source)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        if (format.Length == 0)
            return string.Empty;

        var result = new StringBuilder(format.Length * 2);
        var expression = new StringBuilder();

        var e = format.GetEnumerator();
        while (e.MoveNext())
        {
            var ch = e.Current;
            if (ch == '{')
            {
                // start expression block, continue till closing char
                while (true)
                {
                    // end of format string without closing expression
                    if (!e.MoveNext())
                        throw new FormatException();

                    ch = e.Current;
                    if (ch == '}')
                    {
                        // close expression block, evaluate expression and add to result
                        var value = Evaluate(source, expression.ToString());
                        if (value is not null)
                            result.Append(value);

                        // reset expression buffer
                        expression.Length = 0;
                        break;
                    }
                    if (ch == '{')
                    {
                        // double expression start, add to result
                        result.Append(ch);
                        break;
                    }

                    // add to expression buffer
                    expression.Append(ch);
                }
            }
            else if (ch == '}')
            {
                // close expression char without having started one
                if (!e.MoveNext() || e.Current != '}')
                    throw new FormatException();

                // double expression close, add to result
                result.Append('}');
            }
            else
            {
                // normal char, add to result
                result.Append(ch);
            }
        }

        return result.ToString();
    }

    private static string? Evaluate(object? source, string expression)
    {
        if (source is null)
            return string.Empty;

        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException($"'{nameof(expression)}' cannot be null or empty.", nameof(expression));

        string? format = null;

        // support format string {0:d}
        int colonIndex = expression.IndexOf(':');
        if (colonIndex > 0)
        {
            format = expression.Substring(colonIndex + 1);
            expression = expression.Substring(0, colonIndex);
        }

        // better way to support more dictionary generics?
        if (source is IDictionary<string, string> stringDictionary)
        {
            stringDictionary.TryGetValue(expression, out var value);
            return FormatValue(value, format);
        }
        else if (source is IDictionary<string, object> objectDictionary)
        {
            objectDictionary.TryGetValue(expression, out var value);
            return FormatValue(value, format);
        }
        else if (source is System.Collections.IDictionary dictionary)
        {
            var value = dictionary[expression];
            return FormatValue(value, format);
        }
        else
        {
            var value = GetValue(source, expression);
            return FormatValue(value, format);
        }
    }

    private static object? GetValue(object? target, string name)
    {
        if (target is null)
            return null;

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        var currentType = target.GetType();
        var currentTarget = target;

        PropertyInfo? property = null;

        // optimization if no nested property
        if (!name.Contains('.'))
        {
            property = currentType.GetRuntimeProperty(name);
            return property?.GetValue(currentTarget);
        }

        // support nested property
        foreach (var part in name.Split('.'))
        {
            if (property is not null)
            {
                // pending property, get value and type
                currentTarget = property.GetValue(currentTarget);
                currentType = property.PropertyType;
            }

            property = currentType.GetRuntimeProperty(part);
        }

        // return last property
        return property?.GetValue(currentTarget);
    }

    private static string? FormatValue<T>(T? value, string? format)
    {
        if (value is null)
            return string.Empty;

        return string.IsNullOrEmpty(format)
          ? value.ToString()
          : string.Format("{0:" + format + "}", value);
    }
}
