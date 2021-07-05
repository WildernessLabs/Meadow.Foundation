using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl362
    {
        /// <summary>
        /// FIFO control bits.
        /// </summary>
        protected static class FIFOControlMasks
        {
            /// <summary>
            /// Disable FIFO mode.
            /// </summary>
            public const byte FIFO_DISABLED = 0x00;

            /// <summary>
            /// Enable FiFO oldest saved first mode.
            /// </summary>
            public const byte FIFO_OLDEST_SAVED_FIRST = 0x01;

            /// <summary>
            /// Enable FIFOI stream mode.
            /// </summary>
            public const byte FIFO_STREAM_MODE = 0x02;

            /// <summary>
            /// Enable FIFO triggered mode.
            /// </summary>
            public const byte FIFO_TRIGGERED_MODE = 0x03;

            /// <summary>
            /// When this bit is set to 1, the temperature data is stored in the FIFO
            /// buffer as well as the x, y and z axis data.
            /// </summary>
            public const byte STORE_TEMPERATURE_DATA = 0x04;

            /// <summary>
            /// MSB of the FIFO sample count.  This allows the FIFO buffer to contain 512 samples.
            /// </summary>
            public const byte ABOVE_HALF = 0x08;
        }
    }
}
