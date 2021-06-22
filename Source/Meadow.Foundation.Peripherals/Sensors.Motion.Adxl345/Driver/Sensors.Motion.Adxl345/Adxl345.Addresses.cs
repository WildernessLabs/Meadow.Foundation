using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl345
    {
        /// <summary>
        /// Valid I2C addresses for the sensor.
        /// </summary>
        public static class Addresses
        {
            public const byte Low = 0x53;
            public const byte High = 0x1D;
        }
    }
}
