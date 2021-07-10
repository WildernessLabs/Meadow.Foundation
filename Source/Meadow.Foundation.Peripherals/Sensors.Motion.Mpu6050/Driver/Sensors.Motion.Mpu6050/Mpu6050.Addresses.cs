using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mpu6050
    {
        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public static class Addresses
        {
            public const byte Low = 0x68;
            public const byte High = 0x69;
        }
    }
}
