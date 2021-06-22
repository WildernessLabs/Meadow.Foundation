using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl362
    {
        /// <summary>
        /// Bit masks for the interrupt 1 / 2 control.
        /// </summary>
        public static class InterruptMasks
        {
            /// <summary>
            /// Bit indicating that data is ready for processing.
            /// </summary>
            public const byte DataReady = 0x01;

            /// <summary>
            /// Bit indicating that data is ready in the FIFO buffer.
            /// </summary>
            public const byte FIFODataReady = 0x02;

            /// <summary>
            /// Bit indicating that the FIFO buffer has reached the high watermark.
            /// </summary>
            public const byte FIFOHighWatermarkReached = 0x04;

            /// <summary>
            /// Bit indicating that the FIFO buffer has overrun.
            /// </summary>
            public const byte FIFOOverrun = 0x08;

            /// <summary>
            /// Activity interrupt bit.
            /// </summary>
            public const byte Activity = 0x10;

            /// <summary>
            /// Inactivity interrupt.
            /// </summary>
            public const byte Inactivity = 0x20;

            /// <summary>
            /// Awake interrupt.
            /// </summary>
            public const byte Awake = 0x40;

            /// <summary>
            /// Interrupt active high / low (1 = low, 0 = high).
            /// </summary>
            public const byte ActiveLow = 0x80;
        }
    }
}
