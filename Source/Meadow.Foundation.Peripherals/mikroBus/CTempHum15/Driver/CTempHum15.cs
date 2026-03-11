using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;

namespace Meadow.Foundation.mikroBUS.Sensors.Atmospheric
{
    /// <summary>
    /// Represents a mikroBUS Temp and Hum 15 Click board
    /// </summary>
    public class CTempHum15 : Sht4x
    {
        /// <summary>
        /// Creates a CTempHum15 driver
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        public CTempHum15(II2cBus i2cBus) : base(i2cBus, (byte)Addresses.Default)
        { }

        /// <summary>
        /// Creates a CTempHum15 driver using a MikroBus connector
        /// </summary>
        /// <param name="connector">The MikroBus connector</param>
        public CTempHum15(MikroBusConnector connector) : base(connector.I2cBus, (byte)Addresses.Default)
        { }
    }
}