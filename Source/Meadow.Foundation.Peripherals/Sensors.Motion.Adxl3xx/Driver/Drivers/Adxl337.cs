﻿using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Driver for the ADXL337 triple axis accelerometer
    /// +/- 3g
    /// </summary>
    public class Adxl337 : Adxl3xxBase
    {
        /// <summary>
        /// Create a new ADXL335 sensor object
        /// </summary>
        /// <param name="xPin">Analog pin connected to the X axis output from the ADXL335 sensor</param>
        /// <param name="yPin">Analog pin connected to the Y axis output from the ADXL335 sensor</param>
        /// <param name="zPin">Analog pin connected to the Z axis output from the ADXL335 sensor</param>
        /// <param name="supplyVoltage">The voltage supplied to the sensor. Defaults to 3.3V if null</param>
        public Adxl337(IPin xPin, IPin yPin, IPin zPin, Voltage? supplyVoltage)
                : base(xPin, yPin, zPin, 6, supplyVoltage)
        { }
    }
}