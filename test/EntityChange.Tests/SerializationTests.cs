using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using VerifyXunit;

using Xunit;

namespace EntityChange.Tests;

public class SerializationTests
{
    [Fact]
    public async Task SerializeTest()
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

        await Verifier.Verify(json).UseDirectory("Snapshots");
    }

    [Fact]
    public async Task SerializeNullsTest()
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

        await Verifier.Verify(json).UseDirectory("Snapshots");
    }

    [Fact]
    public void DeserializeTest()
    {
        var json = "[{\"propertyName\":\"Description\",\"displayName\":\"Description\",\"path\":\"Description\",\"operation\":2,\"originalValue\":\"Case Management 15 Min\",\"currentValue\":\"Case Management - 15 Min\",\"originalFormatted\":\"Case Management 15 Min\",\"currentFormatted\":\"Case Management - 15 Min\"},{\"propertyName\":\"CustomerType\",\"displayName\":\"Customer Type\",\"path\":\"CustomerType\",\"operation\":2,\"originalValue\":null,\"currentValue\":\"060\",\"originalFormatted\":\"\",\"currentFormatted\":\"060\"}]";

        var changes = JsonSerializer.Deserialize<List<ChangeRecord>>(json);

        Assert.NotNull(changes);
        Assert.Equal(2, changes.Count);
    }

    [Fact]
    public void DeserializeEnumStringTest()
    {
        var json = "[{\"propertyName\":\"Name\",\"displayName\":\"Name\",\"path\":\"Name\",\"operation\":\"Replace\",\"originalValue\":\"Name 1\",\"currentValue\":\"Name 2\",\"originalFormatted\":\"Name 1\",\"currentFormatted\":\"Name 2\"},{\"propertyName\":\"Description\",\"displayName\":\"Description\",\"path\":\"Description\",\"operation\":\"Replace\",\"originalValue\":\"Test 1\",\"currentValue\":\"Test 2\",\"originalFormatted\":\"Test 1\",\"currentFormatted\":\"Test 2\"}]";

        var changes = JsonSerializer.Deserialize<List<ChangeRecord>>(json);

        Assert.NotNull(changes);
        Assert.Equal(2, changes.Count);
    }
}
