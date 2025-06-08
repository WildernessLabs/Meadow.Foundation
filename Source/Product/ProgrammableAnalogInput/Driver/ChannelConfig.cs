namespace Meadow.Foundation;

public class ChannelConfig
{
    public int ChannelNumber { get; set; }
    public ConfigurableAnalogInputChannelType ChannelType { get; set; }
    public double Scale { get; set; } = 1.0;
    public double Offset { get; set; } = 0.0;
    public string UnitType { get; set; }
    public string Name { get; set; }
}
