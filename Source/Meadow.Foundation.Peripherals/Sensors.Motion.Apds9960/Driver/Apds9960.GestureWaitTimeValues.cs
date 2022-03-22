using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        public static class GestureWaitTimeValues
        {
            /* Gesture wait time values */
            public const byte GWTIME_0MS = 0;
            public const byte GWTIME_2_8MS = 1;
            public const byte GWTIME_5_6MS = 2;
            public const byte GWTIME_8_4MS = 3;
            public const byte GWTIME_14_0MS = 4;
            public const byte GWTIME_22_4MS = 5;
            public const byte GWTIME_30_8MS = 6;
            public const byte GWTIME_39_2MS = 7;
        }
    }
}
