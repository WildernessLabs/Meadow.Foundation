using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        protected static class Registers
        {
            public const byte SYSTEM_CONTROL = 0x40;
            public const byte MODE_CONTROL1 = 0x41;
            public const byte MODE_CONTROL2 = 0x42;
            public const byte MODE_CONTROL3 = 0x44;

            public const byte RED_DATA = 0x50;
            public const byte GREEN_DATA = 0x52;
            public const byte BLUE_DATA = 0x54;
            public const byte CLEAR_DATA = 0x56;
            public const byte DINT_DATA = 0x58;
            public const byte INTERRUPT = 0x60;
            public const byte PERSISTENCE = 0x61;
            public const byte TH = 0x62;
            public const byte TL = 0x64;
            public const byte MANUFACTURER_ID = 0x92;
        }
    }
}
