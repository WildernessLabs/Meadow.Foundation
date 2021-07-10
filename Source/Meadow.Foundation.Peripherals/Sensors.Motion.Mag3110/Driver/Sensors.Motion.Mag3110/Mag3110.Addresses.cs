using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mag3110
    {
        /// <summary>
        /// The MAG3110 is avaible in two parts, each with their own I2C address.
        /// This class contains the part addresses.
        /// </summary>
        public static class Addresses
        {
            public const byte Mag3110 = 0x0E;
            public const byte Fxms3110 = 0x0F;
        }
    }
}
