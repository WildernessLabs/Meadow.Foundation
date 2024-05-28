using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Aht20 Temperature sensor object
/// </summary>    
public class Aht20 : Ahtx0
{
    /// <summary>
    /// Create a new Aht20 object using the default configuration for the sensor
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">I2C address of the sensor</param>
    public Aht20(II2cBus i2cBus, byte address = 56)
        : base(i2cBus, address)
    {
    }
}
