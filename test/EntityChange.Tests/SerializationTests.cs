using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace EntityChange.Tests;

public class SerializationTests
{
    [Fact]
    public void SerializeTest()
    {
        List<ChangeRecord> changes =
        [
            new ChangeRecord
            {
                PropertyName = "Name",
                DisplayName = "Name",
                Path = "Name",
                Operation = ChangeOperation.Replace,
                OriginalValue = "Name 1",
                OriginalFormatted = "Name 1",
                CurrentValue = "Name 2",
                CurrentFormatted = "Name 2"
            },
            new ChangeRecord
            {
                PropertyName = "Description",
                DisplayName = "Description",
                Path = "Description",
                Operation = ChangeOperation.Replace,
                OriginalValue = "Test 1",
                OriginalFormatted = "Test 1",
                CurrentValue = "Test 2",
                CurrentFormatted = "Test 2"
            },
        ];

        var json = JsonSerializer.Serialize(changes);

        const string expected =
            """
            [
              {
                "propertyName": "Name",
                "displayName": "Name",
                "path": "Name",
                "operation": "Replace",
                "originalValue": "Name 1",
                "currentValue": "Name 2",
                "originalFormatted": "Name 1",
                "currentFormatted": "Name 2"
              },
              {
                "propertyName": "Description",
                "displayName": "Description",
                "path": "Description",
                "operation": "Replace",
                "originalValue": "Test 1",
                "currentValue": "Test 2",
                "originalFormatted": "Test 1",
                "currentFormatted": "Test 2"
              }
            ]
            """;

        JsonAssert.Equal(expected, json);
    }

    [Fact]
    public void SerializeNullsTest()
    {
        List<ChangeRecord> changes =
        [
            new ChangeRecord
            {
                PropertyName = "Name",
                DisplayName = "Name",
                Path = "Name",
                Operation = ChangeOperation.Replace,
                OriginalValue = "Name 1",
                OriginalFormatted = "Name 1",
                CurrentValue = null,
                CurrentFormatted = null
            },
            new ChangeRecord
            {
                PropertyName = "Description",
                DisplayName = "Description",
                Path = "Description",
                Operation = ChangeOperation.Replace,
                OriginalValue = null,
                OriginalFormatted = null,
                CurrentValue = "Test 2",
                CurrentFormatted = "Test 2"
            },
        ];

        var json = JsonSerializer.Serialize(changes);

        const string expected =
            """
            [
              {
                "propertyName": "Name",
                "displayName": "Name",
                "path": "Name",
                "operation": "Replace",
                "originalValue": "Name 1",
                "originalFormatted": "Name 1"
              },
              {
                "propertyName": "Description",
                "displayName": "Description",
                "path": "Description",
                "operation": "Replace",
                "currentValue": "Test 2",
                "currentFormatted": "Test 2"
              }
            ]
            """;

        JsonAssert.Equal(expected, json);
    }

    [Fact]
    public void DeserializeTest()
    {
        const string json = "[{\"propertyName\":\"Description\",\"displayName\":\"Description\",\"path\":\"Description\",\"operation\":2,\"originalValue\":\"Case Management 15 Min\",\"currentValue\":\"Case Management - 15 Min\",\"originalFormatted\":\"Case Management 15 Min\",\"currentFormatted\":\"Case Management - 15 Min\"},{\"propertyName\":\"CustomerType\",\"displayName\":\"Customer Type\",\"path\":\"CustomerType\",\"operation\":2,\"originalValue\":null,\"currentValue\":\"060\",\"originalFormatted\":\"\",\"currentFormatted\":\"060\"}]";

        var changes = JsonSerializer.Deserialize<List<ChangeRecord>>(json);

        Assert.NotNull(changes);
        Assert.Equal(2, changes.Count);
    }

    [Fact]
    public void DeserializeEnumStringTest()
    {
        const string json = "[{\"propertyName\":\"Name\",\"displayName\":\"Name\",\"path\":\"Name\",\"operation\":\"Replace\",\"originalValue\":\"Name 1\",\"currentValue\":\"Name 2\",\"originalFormatted\":\"Name 1\",\"currentFormatted\":\"Name 2\"},{\"propertyName\":\"Description\",\"displayName\":\"Description\",\"path\":\"Description\",\"operation\":\"Replace\",\"originalValue\":\"Test 1\",\"currentValue\":\"Test 2\",\"originalFormatted\":\"Test 1\",\"currentFormatted\":\"Test 2\"}]";

        var changes = JsonSerializer.Deserialize<List<ChangeRecord>>(json);

        Assert.NotNull(changes);
        Assert.Equal(2, changes.Count);
    }

    [Fact]
    public void SerializeValueTypesTest()
    {
        var changes = CreateValueTypeChanges();

        var json = JsonSerializer.Serialize(changes);

        const string expected =
            """
            [
              {
                "propertyName": "Property0",
                "displayName": "Property 0",
                "path": "Property0",
                "operation": "Replace",
                "currentValue": 123,
                "currentFormatted": "123"
              },
              {
                "propertyName": "Property1",
                "displayName": "Property 1",
                "path": "Property1",
                "operation": "Replace",
                "currentValue": 123.45,
                "currentFormatted": "123.45"
              },
              {
                "propertyName": "Property2",
                "displayName": "Property 2",
                "path": "Property2",
                "operation": "Replace",
                "currentValue": true,
                "currentFormatted": "True"
              },
              {
                "propertyName": "Property3",
                "displayName": "Property 3",
                "path": "Property3",
                "operation": "Replace",
                "currentValue": "6f9619ff-8b86-d011-b42d-00c04fc964ff",
                "currentFormatted": "6f9619ff-8b86-d011-b42d-00c04fc964ff"
              },
              {
                "propertyName": "Property4",
                "displayName": "Property 4",
                "path": "Property4",
                "operation": "Replace",
                "currentValue": "2024-05-17T14:30:00Z",
                "currentFormatted": "05/17/2024 14:30:00"
              },
              {
                "propertyName": "Property5",
                "displayName": "Property 5",
                "path": "Property5",
                "operation": "Replace",
                "currentValue": "2024-05-17T14:30:00-05:00",
                "currentFormatted": "05/17/2024 14:30:00 -05:00"
              },
              {
                "propertyName": "Property6",
                "displayName": "Property 6",
                "path": "Property6",
                "operation": "Replace",
                "currentValue": "01:02:03",
                "currentFormatted": "01:02:03"
              },
              {
                "propertyName": "Property7",
                "displayName": "Property 7",
                "path": "Property7",
                "operation": "Replace",
                "currentValue": "2024-05-17",
                "currentFormatted": "05/17/2024"
              },
              {
                "propertyName": "Property8",
                "displayName": "Property 8",
                "path": "Property8",
                "operation": "Replace",
                "currentValue": "14:30:00.0000000",
                "currentFormatted": "14:30"
              },
              {
                "propertyName": "Property9",
                "displayName": "Property 9",
                "path": "Property9",
                "operation": "Replace",
                "currentValue": "Replace",
                "currentFormatted": "Replace"
              },
              {
                "propertyName": "Property10",
                "displayName": "Property 10",
                "path": "Property10",
                "operation": "Replace",
                "currentValue": "AQID",
                "currentFormatted": "System.Byte[]"
              },
              {
                "propertyName": "Property11",
                "displayName": "Property 11",
                "path": "Property11",
                "operation": "Replace",
                "currentValue": "7:Complex",
                "currentFormatted": "7:Complex"
              }
            ]
            """;

        JsonAssert.Equal(expected, json);
    }

    [Fact]
    public void SerializeValueTypesWithContextTest()
    {
        var changes = CreateValueTypeChanges();

        var json = JsonSerializer.Serialize(changes, EntityChangeSerializerContext.Default.ListChangeRecord);

        const string expected =
            """
            [
              {
                "propertyName": "Property0",
                "displayName": "Property 0",
                "path": "Property0",
                "operation": "Replace",
                "currentValue": 123,
                "currentFormatted": "123"
              },
              {
                "propertyName": "Property1",
                "displayName": "Property 1",
                "path": "Property1",
                "operation": "Replace",
                "currentValue": 123.45,
                "currentFormatted": "123.45"
              },
              {
                "propertyName": "Property2",
                "displayName": "Property 2",
                "path": "Property2",
                "operation": "Replace",
                "currentValue": true,
                "currentFormatted": "True"
              },
              {
                "propertyName": "Property3",
                "displayName": "Property 3",
                "path": "Property3",
                "operation": "Replace",
                "currentValue": "6f9619ff-8b86-d011-b42d-00c04fc964ff",
                "currentFormatted": "6f9619ff-8b86-d011-b42d-00c04fc964ff"
              },
              {
                "propertyName": "Property4",
                "displayName": "Property 4",
                "path": "Property4",
                "operation": "Replace",
                "currentValue": "2024-05-17T14:30:00Z",
                "currentFormatted": "05/17/2024 14:30:00"
              },
              {
                "propertyName": "Property5",
                "displayName": "Property 5",
                "path": "Property5",
                "operation": "Replace",
                "currentValue": "2024-05-17T14:30:00-05:00",
                "currentFormatted": "05/17/2024 14:30:00 -05:00"
              },
              {
                "propertyName": "Property6",
                "displayName": "Property 6",
                "path": "Property6",
                "operation": "Replace",
                "currentValue": "01:02:03",
                "currentFormatted": "01:02:03"
              },
              {
                "propertyName": "Property7",
                "displayName": "Property 7",
                "path": "Property7",
                "operation": "Replace",
                "currentValue": "2024-05-17",
                "currentFormatted": "05/17/2024"
              },
              {
                "propertyName": "Property8",
                "displayName": "Property 8",
                "path": "Property8",
                "operation": "Replace",
                "currentValue": "14:30:00.0000000",
                "currentFormatted": "14:30"
              },
              {
                "propertyName": "Property9",
                "displayName": "Property 9",
                "path": "Property9",
                "operation": "Replace",
                "currentValue": "Replace",
                "currentFormatted": "Replace"
              },
              {
                "propertyName": "Property10",
                "displayName": "Property 10",
                "path": "Property10",
                "operation": "Replace",
                "currentValue": "AQID",
                "currentFormatted": "System.Byte[]"
              },
              {
                "propertyName": "Property11",
                "displayName": "Property 11",
                "path": "Property11",
                "operation": "Replace",
                "currentValue": "7:Complex",
                "currentFormatted": "7:Complex"
              }
            ]
            """;

        JsonAssert.Equal(expected, json);
    }

    [Fact]
    public void DeserializeNumberReturnsInt32Test()
    {
        const string json = "{\"currentValue\":123}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(123, change.CurrentValue);
    }

    [Fact]
    public void DeserializeLargeNumberReturnsInt64Test()
    {
        const string json = "{\"currentValue\":9223372036854775807}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(long.MaxValue, change.CurrentValue);
    }

    [Fact]
    public void DeserializeDecimalNumberReturnsDecimalTest()
    {
        const string json = "{\"currentValue\":123.45}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(123.45m, change.CurrentValue);
    }

    [Fact]
    public void DeserializeBooleanReturnsBooleanTest()
    {
        const string json = "{\"currentValue\":true}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(true, change.CurrentValue);
    }

    [Fact]
    public void DeserializeTimeOnlyReturnsTimeOnlyTest()
    {
        const string json = "{\"currentValue\":\"14:30:00.0000000\"}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(new TimeOnly(14, 30, 0), change.CurrentValue);
    }

    [Fact]
    public void DeserializeDateOnlyReturnsDateOnlyTest()
    {
        const string json = "{\"currentValue\":\"2024-05-17\"}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(new DateOnly(2024, 5, 17), change.CurrentValue);
    }

    [Fact]
    public void DeserializeGuidReturnsGuidTest()
    {
        const string json = "{\"currentValue\":\"6f9619ff-8b86-d011-b42d-00c04fc964ff\"}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(new Guid("6f9619ff-8b86-d011-b42d-00c04fc964ff"), change.CurrentValue);
    }

    [Fact]
    public void DeserializeDateTimeReturnsDateTimeTest()
    {
        const string json = "{\"currentValue\":\"2024-05-17T14:30:00\"}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(new DateTime(2024, 5, 17, 14, 30, 0), change.CurrentValue);
    }

    [Fact]
    public void DeserializeDateTimeOffsetReturnsDateTimeOffsetTest()
    {
        const string json = "{\"currentValue\":\"2024-05-17T14:30:00-05:00\"}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(new DateTimeOffset(2024, 5, 17, 14, 30, 0, TimeSpan.FromHours(-5)), change.CurrentValue);
    }

    [Fact]
    public void DeserializeTimeSpanReturnsTimeSpanTest()
    {
        const string json = "{\"currentValue\":\"1.02:03:04\"}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal(new TimeSpan(1, 2, 3, 4), change.CurrentValue);
    }

    [Fact]
    public void DeserializeTextReturnsStringTest()
    {
        const string json = "{\"currentValue\":\"Name 1\"}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.Equal("Name 1", change.CurrentValue);
    }

    [Fact]
    public void DeserializeObjectReturnsJsonElementTest()
    {
        const string json = "{\"currentValue\":{\"id\":7}}";

        var change = JsonSerializer.Deserialize<ChangeRecord>(json);

        Assert.NotNull(change);
        Assert.IsType<JsonElement>(change.CurrentValue);
    }

    private static List<ChangeRecord> CreateValueTypeChanges()
    {
        object[] values =
        [
            123,
            123.45m,
            true,
            new Guid("6f9619ff-8b86-d011-b42d-00c04fc964ff"),
            new DateTime(2024, 5, 17, 14, 30, 0, DateTimeKind.Utc),
            new DateTimeOffset(2024, 5, 17, 14, 30, 0, TimeSpan.FromHours(-5)),
            new TimeSpan(1, 2, 3),
            new DateOnly(2024, 5, 17),
            new TimeOnly(14, 30, 0),
            ChangeOperation.Replace,
            new byte[] { 1, 2, 3 },
            new ComplexValue { Id = 7, Name = "Complex" },
        ];

        var changes = new List<ChangeRecord>();

        for (var index = 0; index < values.Length; index++)
        {
            changes.Add(new ChangeRecord
            {
                PropertyName = $"Property{index}",
                DisplayName = $"Property {index}",
                Path = $"Property{index}",
                Operation = ChangeOperation.Replace,
                OriginalValue = null,
                OriginalFormatted = null,
                CurrentValue = values[index],
                // format with invariant culture so expected values are stable across platforms
                CurrentFormatted = Convert.ToString(values[index], CultureInfo.InvariantCulture),
            });
        }

        return changes;
    }

    private sealed class ComplexValue
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public override string ToString() => $"{Id}:{Name}";
    }
}
