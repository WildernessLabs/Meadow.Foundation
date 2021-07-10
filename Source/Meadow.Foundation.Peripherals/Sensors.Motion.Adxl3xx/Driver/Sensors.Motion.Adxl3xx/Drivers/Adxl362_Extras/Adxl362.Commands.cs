using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl362
    {
        /// <summary>
        /// Command byte (first byte in any communication).
        /// </summary>
        protected static class Commands
        {
            /// <summary>
            /// Write to one or more registers.
            /// </summary>
            public const byte WRITE_REGISTER = 0x0a;

            /// <summary>
            /// Read the contents of one or more registers.
            /// </summary>
            public const byte READ_REGISTER = 0x0b;

            /// <summary>
            /// Read the FIFO buffer.
            /// </summary>
            public const byte READ_FIFO = 0x0d;
        }
    }
}