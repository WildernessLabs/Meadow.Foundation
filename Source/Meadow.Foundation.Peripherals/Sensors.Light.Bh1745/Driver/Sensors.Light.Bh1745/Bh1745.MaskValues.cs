using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        protected static class MaskValues
        {
            public const byte PART_ID = 0x3F;
            public const byte SW_RESET = 0x80;
            public const byte INT_RESET = 0x40;
            public const byte MEASUREMENT_TIME = 0x07;
            public const byte VALID = 0x80;
            public const byte RGBC_EN = 0x10;
            public const byte ADC_GAIN = 0x03;
            public const byte INT_STATUS = 0x80;
            public const byte INT_LATCH = 0x10;
            public const byte INT_SOURCE = 0x0C;
            public const byte INT_ENABLE = 0x01;
            public const byte PERSISTENCE_MASK = 0x03;
        }
    }
}
