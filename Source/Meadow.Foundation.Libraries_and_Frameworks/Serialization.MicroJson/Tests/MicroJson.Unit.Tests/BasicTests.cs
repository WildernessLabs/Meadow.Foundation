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
}
