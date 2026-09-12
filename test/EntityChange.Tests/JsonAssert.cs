using System.Collections.Generic;
using System.Text.Json;

namespace EntityChange.Tests;

internal static class JsonAssert
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public static void Equal(string expected, IEnumerable<ChangeRecord> changes)
        => Equal(expected, JsonSerializer.Serialize(changes, SerializerOptions));

    public static void Equal(string expected, string actual)
        => Assert.Equal(Normalize(expected), Normalize(actual));

    // reformat json so differences in whitespace don't cause failures
    private static string Normalize(string value)
    {
        var text = value.Replace("\r\n", "\n").Trim();

        using var document = JsonDocument.Parse(text);
        return JsonSerializer.Serialize(document, SerializerOptions).Replace("\r\n", "\n");
    }
}
