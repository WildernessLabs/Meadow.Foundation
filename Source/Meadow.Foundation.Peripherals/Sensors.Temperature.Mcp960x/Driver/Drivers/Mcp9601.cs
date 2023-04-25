using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Represents a Mcp9601 Thermocouple sensor object
    /// </summary>    
    public partial class Mcp9601 : Mcp960x
    {
        /// <summary>
        /// Create a new Mcp9601 object using the default configuration for the sensor
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">I2C address of the sensor</param>
        public Mcp9601(II2cBus i2cBus, byte address = (byte)Address.Default) : base(i2cBus, address)
        {
        }
    }
}