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
            public const byte DATA_READY = 0x01;

            /// <summary>
            /// Bit indicating that data is ready in the FIFO buffer.
            /// </summary>
            public const byte FIFO_DATA_READY = 0x02;

            /// <summary>
            /// Bit indicating that the FIFO buffer has reached the high watermark.
            /// </summary>
            public const byte FIFO_HIGH_WATERMARK_REACHED = 0x04;

            /// <summary>
            /// Bit indicating that the FIFO buffer has overrun.
            /// </summary>
            public const byte FIFO_OVERRUN = 0x08;

            /// <summary>
            /// Activity interrupt bit.
            /// </summary>
            public const byte ACTIVITY = 0x10;

            /// <summary>
            /// Inactivity interrupt.
            /// </summary>
            public const byte INACTIVITY = 0x20;

            /// <summary>
            /// Awake interrupt.
            /// </summary>
            public const byte AWAKE = 0x40;

            /// <summary>
            /// Interrupt active high / low (1 = low, 0 = high).
            /// </summary>
            public const byte ACTIVE_LOW = 0x80;
        }
    }
}
