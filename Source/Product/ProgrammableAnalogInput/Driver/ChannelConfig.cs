namespace Meadow.Foundation;

/// <summary>
/// Configuration for a single analog input channel
/// </summary>
public class ChannelConfig
{
    /// <summary>
    /// Gets or sets the zero-based channel index
    /// </summary>
    public int ChannelNumber { get; set; }

    /// <summary>
    /// Gets or sets the signal type configured for this channel
    /// </summary>
    public ConfigurableAnalogInputChannelType ChannelType { get; set; }

    /// <summary>
    /// Gets or sets the scale factor applied to the converted unit value
    /// </summary>
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the offset applied to the converted unit value after scaling
    /// </summary>
    public double Offset { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the unit type name used to convert the raw signal to an engineering unit
    /// </summary>
    public string UnitType { get; set; }

    /// <summary>
    /// Gets or sets the display name for this channel
    /// </summary>
    public string Name { get; set; }
}
