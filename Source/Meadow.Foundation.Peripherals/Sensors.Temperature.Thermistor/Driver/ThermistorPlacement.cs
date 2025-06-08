namespace Meadow.Foundation.Sensors.Temperature;

/// <summary>
/// Represents the connection placement of the thermistor in the voltage divider
/// </summary>
public enum ThermistorPlacement
{
    /// <summary>
    /// Thermistor is connected between the analog input and the reference voltage (high-side)
    /// </summary>
    HighSide,

    /// <summary>
    /// Thermistor is connected between the analog input and ground (low-side)
    /// </summary>
    LowSide
}