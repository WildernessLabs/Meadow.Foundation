namespace Meadow.Foundation;

/// <summary>
/// Defines the signal type a programmable analog input channel is configured to measure
/// </summary>
public enum ConfigurableAnalogInputChannelType
{
    /// <summary>0-10V voltage input</summary>
    Voltage_0_10,
    /// <summary>4-20mA current loop input</summary>
    Current_4_20,
    /// <summary>0-20mA current loop input</summary>
    Current_0_20,
    /// <summary>NTC thermistor input</summary>
    ThermistorNtc
}
