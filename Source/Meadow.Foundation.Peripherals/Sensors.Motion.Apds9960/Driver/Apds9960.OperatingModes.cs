using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        public static class OperatingModes
        {
            /* Acceptable parameters for setMode */
            public const byte POWER = 0;
            public const byte AMBIENT_LIGHT = 1;
            public const byte PROXIMITY = 2;
            public const byte WAIT = 3;
            public const byte AMBIENT_LIGHT_INT = 4;
            public const byte PROXIMITY_INT = 5;
            public const byte GESTURE = 6;
            public const byte ALL = 7;
        }
    }
}