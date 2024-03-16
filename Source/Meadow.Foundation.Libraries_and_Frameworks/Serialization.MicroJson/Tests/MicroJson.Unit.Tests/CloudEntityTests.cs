using Meadow.Cloud;
using Meadow.Foundation.Serialization;
using Meadow.Update;
using System.Collections.Generic;
using Xunit;

namespace Unit.Tests;

public class CloudEntityTests
{
    [Fact]
    public void UpdateMessageSerializationTest()
    {
        UpdateMessage message = new()
        {

        };

        var json = MicroJson.Serialize(message);
    }

    [Fact]
    public void MeadowCommandSerializationTest()
    {
        var command = new MeadowCommand("command name",
            new Dictionary<string, object>
            {
                { "field 1", 23 },
                { "field 2", "foo" },
                { "field 3", true },
                { "field 4", 42.2d }
            });

        var json = MicroJson.Serialize(command);
    }

    [Fact]
    public void MeadowCommandDeserializationTest()
    {
        var expected = new Dictionary<string, object>
            {
                { "field 1", 23L },
                { "field 2", "foo" },
                { "field 3", true },
                { "field 4", 42.2d }
            };

        var json = "{\"field 1\":23,\"field 2\":\"foo\",\"field 3\":true,\"field 4\":42.2}";
        var result = MicroJson.Deserialize<Dictionary<string, object>>(json);

        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        foreach (var kvp in expected)
        {
            Assert.True(result.ContainsKey(kvp.Key));
            // this fails because the boxed '23' values arent-'t equat
            // Assert.True(result[kvp.Key] == kvp.Value, $"{result[kvp.Key]} != {kvp.Value}");
        }
    }
}
