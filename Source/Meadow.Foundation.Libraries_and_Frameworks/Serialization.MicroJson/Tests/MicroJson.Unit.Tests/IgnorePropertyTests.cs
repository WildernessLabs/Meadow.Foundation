using Meadow.Foundation.Serialization;
using Xunit;

namespace Unit.Tests;

public class IgnorePropertyTests
{
    [Fact]
    public void SkipDeserializingIgnoredPropertyTest()
    {
        var input = """
            {
                "ValueA": 23
                "ValueB": "This should not appear"
                "ValueC": true
            }
            """;

        var result = MicroJson.Deserialize<IgnorableContainerClass>(input);

        Assert.Null(result.ValueB);
    }

    [Fact]
    public void SkipSerializingIgnoredPropertyTest()
    {
        var item = new IgnorableContainerClass
        {
            ValueA = 42,
            ValueB = "This should not appear",
            ValueC = true
        };

        var result = MicroJson.Serialize(item);

        Assert.DoesNotContain("ValueB", result);
    }
}
