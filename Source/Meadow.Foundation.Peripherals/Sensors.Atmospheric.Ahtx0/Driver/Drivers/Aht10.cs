using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Aht10 Temperature sensor object
/// </summary>    
public class Aht10 : Ahtx0
{
    /// <summary>
    /// Create a new Aht10 object using the default configuration for the sensor
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">I2C address of the sensor</param>
    public Aht10(II2cBus i2cBus, byte address = 56)
        : base(i2cBus, address)
    {
    }
}
