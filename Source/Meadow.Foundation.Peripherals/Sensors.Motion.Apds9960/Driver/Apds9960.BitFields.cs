using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        public static class BitFields
        {
            /* Bit fields */
            public const byte APDS9960_PON = 0b00000001;
            public const byte APDS9960_AEN = 0b00000010;
            public const byte APDS9960_PEN = 0b00000100;
            public const byte APDS9960_WEN = 0b00001000;
            public const byte APSD9960_AIEN = 0b00010000;
            public const byte APDS9960_PIEN = 0b00100000;
            public const byte APDS9960_GEN = 0b01000000;
            public const byte APDS9960_GVALID = 0b00000001;
        }
    }
}
