namespace Meadow.Foundation.Sensors.Motion;

public partial class Apds9960
{
    private static class GestureParameters
    {
        /* Gesture parameters */
        public const byte THRESHOLD_OUT = 10;
        public const byte SENSITIVITY_1 = 50;
        public const byte SENSITIVITY_2 = 20;
    }
}