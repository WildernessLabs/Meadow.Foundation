using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl362
    {
        /// <summary>
        /// Masks for the bit in the filter control register.
        /// </summary>
        public static class FilterControlMasks
        {
            /// <summary>
            /// Data rate of 12.5Hz
            /// </summary>
            public const byte DataRate12HalfHz = 0x00;

            /// <summary>
            /// Data rate of 25 Hz
            /// </summary>
            public const byte DataRate25Hz = 0x01;

            /// <summary>
            /// Data rate of 50 Hz.
            /// </summary>
            public const byte DataRate50Hz = 0x02;

            /// <summary>
            /// Data rate 100 Hz.
            /// </summary>
            public const byte DataRate100Hz = 0x03;

            /// <summary>
            /// Data rate of 200 Hz.
            /// </summary>
            public const byte DataRate200Hz = 0x04;

            /// <summary>
            /// Data rate of 400 Hz
            /// </summary>
            public const byte DataRate400Hz = 0x05;

            /// <summary>
            /// Enable the external sampling trigger.
            /// </summary>
            /// <remarks>
            /// Setting this bit to 1 enables the sampling to be controlled by the INT2 pin.
            /// </remarks>
            public const byte ExternalSampling = 0x08;

            /// <summary>
            /// Half or quarter bandwidth.
            /// </summary>
            /// <remarks>
            /// Setting this bit to 1 changes the anti-aliasing filters from 1/2 the output
            /// data rate to 1/4 the output data rate.
            /// </remarks>
            public const byte HalfBandwidth = 0x10;

            /// <summary>
            /// Set the range to +/- 2g.
            /// </summary>
            public const byte Range2g = 0x00;

            /// <summary>
            /// Set the range to +/- 4g
            /// </summary>
            public const byte Range4g = 0x40;

            /// <summary>
            /// Set the range to +/- 8g
            /// </summary>
            public const byte Range8g = 0x80;
        }
    }
}
