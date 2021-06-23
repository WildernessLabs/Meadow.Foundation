using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl362
    {
        /// <summary>
        /// Masks for the bits in the Power Control register.
        /// </summary>
        protected static class PowerControlMasks
        {
            /// <summary>
            /// Place the sensor inStandby.
            /// </summary>
            public const byte STANDBY = 0x00;

            /// <summary>
            /// Make measurements.
            /// </summary>
            public const byte MEASURE = 0x01;

            /// <summary>
            /// Auto-sleep.
            /// </summary>
            public const byte AUTOSLEEP = 0x04;

            /// <summary>
            /// Wakeup mode.
            /// </summary>
            public const byte WAKEUP_MODE = 0x08;

            /// <summary>
            /// Low noise mode.
            /// </summary>
            public const byte LOW_NOISE = 0x10;

            /// <summary>
            /// Ultra-low noise mode.
            /// </summary>
            public const byte ULTRALOW_NOISE = 0x20;

            /// <summary>
            /// External clock enabled on the INT1 pin.
            /// </summary>
            public const byte EXTERNAL_CLOCK = 0x40;
        }
    }
}
