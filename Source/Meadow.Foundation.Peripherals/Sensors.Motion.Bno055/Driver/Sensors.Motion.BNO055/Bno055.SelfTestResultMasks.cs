using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Bno055
    {
        /// <summary>
        ///     Self test result bit mask.
        /// </summary>
        public static class SelfTestResultMasks
        {
            /// <summary>
            ///     Accelerometer bit mask.
            /// </summary>
            public const byte ACCELEROMETER = 0x01;

            /// <summary>
            ///     Magnetometer bit mask.
            /// </summary>
            public const byte MAGNETOMETER = 0x02;

            /// <summary>
            ///     Gyroscope bit mask.
            /// </summary>
            public const byte GYROSCOPE = 0x04;

            /// <summary>
            ///     Microcontroller bit mask.
            /// </summary>
            public const byte MICROCONTROLLER = 0x08;
        }
    }
}
