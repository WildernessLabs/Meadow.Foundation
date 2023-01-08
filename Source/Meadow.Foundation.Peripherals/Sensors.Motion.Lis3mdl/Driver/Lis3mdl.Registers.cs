using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Lis3mdl
    {
        /// <summary>
        /// Register addresses in the sensor
        /// </summary>
        private static class Registers
        {
            public const byte STATUS = 0x27;
            public const byte OUT_X_L = 0x28;
            public const byte TEMP_OUT_L = 0x2E;
            public const byte INT_CONF = 0x30;
            public const byte INT_THIS_L = 0x32;
            public const byte WHO_AM_I = 0x0F;
            public const byte CONTROL_1 = 0x20;
            public const byte CONTROL_2 = 0x21;
            public const byte CONTROL_3 = 0x22;
            public const byte CONTROL_4 = 0x23;
        }
    }
}
