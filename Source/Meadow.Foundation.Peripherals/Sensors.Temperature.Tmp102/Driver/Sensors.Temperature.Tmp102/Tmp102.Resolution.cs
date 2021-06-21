using System;

namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Tmp102
    {
        /// <summary>
        ///     Indicate the resolution of the sensor.
        /// </summary>
        public enum Resolution : byte
        {
            /// <summary>
            ///     Operate in 12-bit mode.
            /// </summary>
            Resolution12Bits,

            /// <summary>
            ///     Operate in 13-bit mode.
            /// </summary>
            Resolution13Bits
        }

    }
}
