using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Si1145
    {
        protected static class Commands
        {
            /* COMMANDS*/
            public static readonly byte PARAM_QUERY = 0x80;
            public static readonly byte PARAM_SET = 0xA0;
            public static readonly byte NOP = 0x00;
            public static readonly byte RESET = 0x01;
            public static readonly byte BUSADDR = 0x02;
            public static readonly byte PS_FORCE = 0x05;
            public static readonly byte ALS_FORCE = 0x06;
            public static readonly byte PSALS_FORCE = 0x07;
            public static readonly byte PS_PAUSE = 0x09;
            public static readonly byte ALS_PAUSE = 0x0A;
            public static readonly byte PSALS_PAUSE = 0xB;
            public static readonly byte PS_AUTO = 0x0D;
            public static readonly byte ALS_AUTO = 0x0E;
            public static readonly byte PSALS_AUTO = 0x0F;
            public static readonly byte GET_CAL = 0x12;
        }
    }
}
