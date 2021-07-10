using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        public static class GainValues
        {
            /* Proximity Gain (PGAIN) values */
            public const byte PGAIN_1X = 0;
            public const byte PGAIN_2X = 1;
            public const byte PGAIN_4X = 2;
            public const byte PGAIN_8X = 3;

            /* ALS Gain (AGAIN) values */
            public const byte AGAIN_1X = 0;
            public const byte AGAIN_4X = 1;
            public const byte AGAIN_16X = 2;
            public const byte AGAIN_64X = 3;

            /* Gesture Gain (GGAIN) values */
            public const byte GGAIN_1X = 0;
            public const byte GGAIN_2X = 1;
            public const byte GGAIN_4X = 2;
            public const byte GGAIN_8X = 3;

            /* LED Boost values */
            public const byte LED_BOOST_100 = 0;
            public const byte LED_BOOST_150 = 1;
            public const byte LED_BOOST_200 = 2;
            public const byte LED_BOOST_300 = 3;
        }
    }
}