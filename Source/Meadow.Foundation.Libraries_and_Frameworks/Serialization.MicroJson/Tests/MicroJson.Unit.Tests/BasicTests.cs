using Meadow.Foundation.Serialization;
using Xunit;

namespace Unit.Tests;

public class BasicTests
{
    [Fact]
    public void SimpleIntegerPropertyTest()
    {
        var input = """
            {
                "Value": 23
            }
            """;

        var result = MicroJson.Deserialize<IntegerClass>(input);

        Assert.Equal(23, result.Value);
    }

    [Fact]
    public void SimpleStringArrayTest()
    {
        var input = """
            [
                "Value1",
                "Value2",
                "Value3"
            ]
            """;

        var result = MicroJson.Deserialize<string[]>(input);

        Assert.Equal(3, result.Length);
    }
}
