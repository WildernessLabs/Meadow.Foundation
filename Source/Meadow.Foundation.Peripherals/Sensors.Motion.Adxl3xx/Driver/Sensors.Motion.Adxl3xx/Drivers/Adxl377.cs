using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Driver for the ADXL377 triple axis accelerometer.
    /// +/- 200g
    /// </summary>
    public class Adxl377 : Adxl3xxBase
    {
        /// <summary>
        /// Create a new ADXL335 sensor object.
        /// </summary>
        /// <param name="device">Device that has the analog pins.</param>
        /// <param name="xPin">Analog pin connected to the X axis output from the ADXL335 sensor.</param>
        /// <param name="yPin">Analog pin connected to the Y axis output from the ADXL335 sensor.</param>
        /// <param name="zPin">Analog pin connected to the Z axis output from the ADXL335 sensor.</param>
        /// <param name="supplyVoltage">The voltage supplied to the sensor. Defaults to 3.3V if null.</param>
        public Adxl377(IAnalogInputController device,
            IPin xPin, IPin yPin, IPin zPin, Voltage? supplyVoltage)
                : base(device, xPin, yPin, zPin, 400, supplyVoltage)
        {
        }
    }
}
