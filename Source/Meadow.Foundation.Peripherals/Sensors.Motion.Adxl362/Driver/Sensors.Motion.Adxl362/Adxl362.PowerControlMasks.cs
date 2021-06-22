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
            public const byte Standby = 0x00;

            /// <summary>
            /// Make measurements.
            /// </summary>
            public const byte Measure = 0x01;

            /// <summary>
            /// Auto-sleep.
            /// </summary>
            public const byte AutoSleep = 0x04;

            /// <summary>
            /// Wakeup mode.
            /// </summary>
            public const byte WakeupMode = 0x08;

            /// <summary>
            /// Low noise mode.
            /// </summary>
            public const byte LowNoise = 0x10;

            /// <summary>
            /// Ultra-low noise mode.
            /// </summary>
            public const byte UltralowNoise = 0x20;

            /// <summary>
            /// External clock enabled on the INT1 pin.
            /// </summary>
            public const byte ExternalClock = 0x40;
        }
    }
}
