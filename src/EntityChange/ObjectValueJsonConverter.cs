using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EntityChange;

/// <summary>
/// Converts loosely typed <see cref="object"/> values to and from JSON without requiring
/// <see cref="System.Text.Json.Serialization.Metadata.JsonTypeInfo"/> metadata for the runtime type.
/// </summary>
/// <remarks>
/// Values are written directly to the <see cref="Utf8JsonWriter"/> using primitive writer methods, so the
/// converter behaves identically under reflection based and source generated serialization. Complex types
/// have no metadata free representation and are written using their string representation.
/// </remarks>
internal sealed class ObjectValueJsonConverter : JsonConverter<object?>
{
    /// <inheritdoc />
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                return ReadString(ref reader);
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Number:
                return ReadNumber(ref reader);
            default:
                // objects and arrays cannot be mapped back to a known type, preserve the raw json
                using (var document = JsonDocument.ParseValue(ref reader))
                    return document.RootElement.Clone();
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string stringValue:
                writer.WriteStringValue(stringValue);
                break;
            case char charValue:
                writer.WriteStringValue(charValue.ToString());
                break;
            case bool booleanValue:
                writer.WriteBooleanValue(booleanValue);
                break;
            case byte byteValue:
                writer.WriteNumberValue(byteValue);
                break;
            case sbyte signedByteValue:
                writer.WriteNumberValue(signedByteValue);
                break;
            case short shortValue:
                writer.WriteNumberValue(shortValue);
                break;
            case ushort unsignedShortValue:
                writer.WriteNumberValue(unsignedShortValue);
                break;
            case int integerValue:
                writer.WriteNumberValue(integerValue);
                break;
            case uint unsignedIntegerValue:
                writer.WriteNumberValue(unsignedIntegerValue);
                break;
            case long longValue:
                writer.WriteNumberValue(longValue);
                break;
            case ulong unsignedLongValue:
                writer.WriteNumberValue(unsignedLongValue);
                break;
            case float floatValue:
                writer.WriteNumberValue(floatValue);
                break;
            case double doubleValue:
                writer.WriteNumberValue(doubleValue);
                break;
            case decimal decimalValue:
                writer.WriteNumberValue(decimalValue);
                break;
            case DateTime dateTimeValue:
                writer.WriteStringValue(dateTimeValue);
                break;
            case DateTimeOffset dateTimeOffsetValue:
                writer.WriteStringValue(dateTimeOffsetValue);
                break;
            case TimeSpan timeSpanValue:
                writer.WriteStringValue(timeSpanValue.ToString("c", CultureInfo.InvariantCulture));
                break;
#if NET6_0_OR_GREATER
            case DateOnly dateOnlyValue:
                writer.WriteStringValue(dateOnlyValue.ToString("O", CultureInfo.InvariantCulture));
                break;
            case TimeOnly timeOnlyValue:
                writer.WriteStringValue(timeOnlyValue.ToString("O", CultureInfo.InvariantCulture));
                break;
#endif
            case Guid guidValue:
                writer.WriteStringValue(guidValue);
                break;
            case byte[] byteArrayValue:
                writer.WriteBase64StringValue(byteArrayValue);
                break;
            case Enum enumValue:
                writer.WriteStringValue(enumValue.ToString());
                break;
            case JsonElement elementValue:
                elementValue.WriteTo(writer);
                break;
            case IFormattable formattableValue:
                writer.WriteStringValue(formattableValue.ToString(null, CultureInfo.InvariantCulture));
                break;
            default:
                // complex types have no metadata free representation, fall back to the string representation
                writer.WriteStringValue(value.ToString());
                break;
        }
    }


    /// <summary>
    /// Progressively converts a JSON number token to the most specific known type.
    /// </summary>
    /// <returns>
    /// The most specific known type that can represent the JSON number token.
    /// </returns>
    private static object ReadNumber(ref Utf8JsonReader reader)
    {
        // prefer the smallest integral type that fits the value
        if (reader.TryGetInt32(out var integerValue))
            return integerValue;

        if (reader.TryGetInt64(out var longValue))
            return longValue;

        if (reader.TryGetDecimal(out var decimalValue))
            return decimalValue;

        return reader.GetDouble();
    }

    /// <summary>
    /// Progressively converts a JSON string token to the most specific known type.
    /// </summary>
    /// <remarks>
    /// Values written by <see cref="Write"/> use round trippable formats, allowing <see cref="Guid"/>,
    /// date, time and duration values to be restored instead of remaining a <see cref="string"/>.
    /// </remarks>
    private static object? ReadString(ref Utf8JsonReader reader)
    {
        if (reader.TryGetGuid(out var guidValue))
            return guidValue;

        var stringValue = reader.GetString();
        if (string.IsNullOrEmpty(stringValue))
            return stringValue;

#if NET6_0_OR_GREATER
        // date only has no time component, check before date time to avoid a midnight DateTime
        if (DateOnly.TryParseExact(stringValue, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnlyValue))
            return dateOnlyValue;

        if (TimeOnly.TryParseExact(stringValue, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeOnlyValue))
            return timeOnlyValue;
#endif

        if (HasTimeZone(stringValue!))
        {
            if (reader.TryGetDateTimeOffset(out var dateTimeOffsetValue))
                return dateTimeOffsetValue;
        }
        else if (reader.TryGetDateTime(out var dateTimeValue))
        {
            return dateTimeValue;
        }

        if (TimeSpan.TryParseExact(stringValue, "c", CultureInfo.InvariantCulture, out var timeSpanValue))
            return timeSpanValue;

        return stringValue;
    }

    /// <summary>
    /// Determines whether the specified value contains a time zone designator, indicating a <see cref="DateTimeOffset"/>.
    /// </summary>
    private static bool HasTimeZone(string value)
    {
        var timeIndex = value.IndexOf('T');
        if (timeIndex < 0)
            return false;

        for (var index = timeIndex + 1; index < value.Length; index++)
        {
            var current = value[index];
            if (current == 'Z' || current == 'z' || current == '+' || current == '-')
                return true;
        }

        return false;
    }
}
