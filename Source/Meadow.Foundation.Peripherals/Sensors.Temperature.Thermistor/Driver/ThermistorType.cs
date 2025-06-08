namespace Meadow.Foundation.Sensors.Temperature;

/// <summary>
/// Represents a thermistor type
/// </summary>
public enum ThermistorType
{
    /// <summary>
    /// Negative Temperature Coefficient - resistance decreases as temperature increases
    /// </summary>
    NTC,

    /// <summary>
    /// Positive Temperature Coefficient - resistance increases as temperature increases
    /// </summary>
    PTC
}
