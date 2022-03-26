using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        public static class GestureParameters
        {
            /* Gesture parameters */
            public const byte GESTURE_THRESHOLD_OUT = 10;
            public const byte GESTURE_SENSITIVITY_1 = 50;
            public const byte GESTURE_SENSITIVITY_2 = 20;
        }
    }
}
