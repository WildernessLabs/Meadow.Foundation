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
        /// <param name="i2cBus"></param>
        public CTempHum15(II2cBus i2cBus) : base(i2cBus, (byte)Addresses.Default)
        { }
    }
}