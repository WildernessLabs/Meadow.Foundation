using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        public static class Addresses
        {
            public const byte DEFAULT_ADDRESS = 0x0D;
            // Note BC: when i found this driver the address was 0x1E, but
            // everything i've read is that it's 0x0D

            public const byte HMC5883_ADDRESS = 0x1E;

            public const byte QMC5883_ADDRESS = 0x0D;
        }
    }
}
