namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Represents the available temperature channels
/// </summary>
public enum TemperatureChannel
{
    /// <summary>
    /// Temperature channel T (1)
    /// </summary>
    T = 1 << 3,
    /// <summary>
    /// Temperature channel TOB1 (2)
    /// </summary>  
    TOB1 = 1 << 4,
    /// <summary>
    /// Temperature channel TOB2 (3)
    /// </summary>
    TOB2 = 1 << 5,
    /// <summary>
    /// Temperature channel Con (4)
    /// </summary>
    Con = 1 << 7
}