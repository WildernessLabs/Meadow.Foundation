using Meadow.Foundation;
using Meadow.Foundation.Serialization;
using Meadow.Units;
using System.Diagnostics;

namespace ProgrammableAnalogInputTests;

public class ConfigSerializationTests
{
    [Fact]
    public void SerializeConfigurationTest()
    {
        // type K thermocoule, 0-100F, (-18-38C)
        // scale = 3.4725°C/mA
        // offset = -31.67°C
        var temperatureConfig = new ChannelConfig
        {
            ChannelType = ConfigurableAnalogInputChannelType.Current_4_20,
            Scale = 6.25,
            Offset = -25,
            UnitType = nameof(Temperature)
        };

        var json = MicroJson.Serialize(temperatureConfig);
        Debug.Write(json);
    }

    [Fact]
    public void DeserializeConfigurationTest()
    {
        var json = "{\"unitType\":\"Temperature\",\"scale\":3.4725,\"channelType\":1,\"offset\":-31.67}";

        var config = MicroJson.Deserialize<ChannelConfig>(json);
        Assert.NotNull(config);
    }
}