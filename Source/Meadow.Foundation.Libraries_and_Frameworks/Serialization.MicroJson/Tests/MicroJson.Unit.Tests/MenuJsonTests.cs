using Meadow.Foundation.Serialization;
using Xunit;

namespace Unit.Tests;

public class MenuJsonTests
{
    [Fact]
    public void DeserializeMenuTest()
    {
        var json = Inputs.GetInputResource("menu.json");
        var result = MicroJson.Deserialize<MenuContainer>(json);

        Assert.NotNull(result);
    }
}
