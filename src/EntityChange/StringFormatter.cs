using System.Text;

namespace EntityChange;

/// <summary>
/// Standard string format options
/// </summary>
public static class StringFormatter
{
    /// <summary>
    /// Format the specified <paramref name="value"/> as a Currency.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? Currency(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDecimal(value);
        return d.ToString("C");
    }

    /// <summary>
    /// Format the specified <paramref name="value"/> as a Number.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? Number(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDecimal(value);
        return d.ToString("N2");
    }


    /// <summary>
    /// Format the specified <paramref name="value"/> as a Short Date pattern.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? ShortDate(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDateTime(value);
        return d.ToString("d");
    }

    /// <summary>
    /// Format the specified <paramref name="value"/> as a Long Date pattern.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? LongDate(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDateTime(value);
        return d.ToString("D");
    }

    /// <summary>
    /// Format the specified <paramref name="value"/> as a Full Date Short Time pattern.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? FullDateShortTime(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDateTime(value);
        return d.ToString("f");
    }

    /// <summary>
    /// Format the specified <paramref name="value"/> as a Full Date Long Time pattern.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? FullDateLongTime(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDateTime(value);
        return d.ToString("F");
    }

    /// <summary>
    /// Format the specified <paramref name="value"/> as a General Date Short Time pattern.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? GeneralDateShortTime(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDateTime(value);
        return d.ToString("g");
    }

    /// <summary>
    /// Format the specified <paramref name="value"/> as a General Date Long Time pattern.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? GeneralDateLongTime(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDateTime(value);
        return d.ToString("G");
    }

    /// <summary>
    /// Format the specified <paramref name="value"/> as a Short Time pattern.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? ShortTime(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDateTime(value);
        return d.ToString("t");
    }

    /// <summary>
    /// Format the specified <paramref name="value"/> as a Long Time pattern.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns></returns>
    public static string? LongTime(object? value)
    {
        if (value is null)
            return null;

        if (value is not IConvertible)
            return value.ToString();

        var d = Convert.ToDateTime(value);
        return d.ToString("T");
    }
}
