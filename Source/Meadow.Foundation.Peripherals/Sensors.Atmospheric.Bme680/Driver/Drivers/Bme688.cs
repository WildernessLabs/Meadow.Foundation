using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// BME688 Temperature, Pressure and Humidity Sensor
    /// </summary>
    /// <remarks>
    /// This class implements the functionality necessary to read the temperature, pressure and humidity
    /// from the Bosch BME688 sensor
    /// </remarks>
    public partial class Bme688 : Bme680
    {
        /// <summary>
        /// Initializes a new instance of the BME688 class
        /// </summary>
        /// <param name="i2cBus">I2C Bus to use for communicating with the sensor</param>
        /// <param name="address">I2C address of the sensor</param>
        public Bme688(II2cBus i2cBus, byte address = 119) : base(i2cBus, address)
        {
        }
    }
}