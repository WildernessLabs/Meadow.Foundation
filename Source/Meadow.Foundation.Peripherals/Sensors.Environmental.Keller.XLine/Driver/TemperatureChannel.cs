namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Represents the available temperature channels
/// </summary>
public enum TemperatureChannel
{
    T = 1 << 3,
    TOB1 = 1 << 4,
    TOB2 = 1 << 5,
    Con = 1 << 7
}