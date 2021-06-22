using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1750
    {
        public class Addresses
        {
            /// <summary>
            /// I2C address when address pin is high
            /// </summary>
            public static byte High => 0x5c;

            /// <summary>
            /// I2C address when address pin is low
            /// </summary>
            public static byte Low => 0x23;
        }
    }
}