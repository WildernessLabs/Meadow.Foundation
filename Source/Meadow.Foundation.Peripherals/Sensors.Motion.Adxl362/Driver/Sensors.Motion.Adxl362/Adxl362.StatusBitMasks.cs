using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl362
    {
        /// <summary>
        /// Status bit mask.
        /// </summary>
        protected static class StatusBitsMasks
        {
            /// <summary>
            /// Indicates if data is ready to be read.
            /// </summary>
            public const byte DataReady = 0x01;

            /// <summary>
            /// Indicate when FIFO data is ready to be read.
            /// </summary>
            public const byte FIFOReady = 0x02;

            /// <summary>
            /// Set when the FIFO watermark has been reached.
            /// </summary>
            public const byte FIFOWatermark = 0x04;

            /// <summary>
            /// True when incoming data is replacing existing data in the FIFO buffer.
            /// </summary>
            public const byte FIFOOverRun = 0x08;

            /// <summary>
            /// Activity has been detected.
            /// </summary>
            public const byte ActivityDetected = 0x10;

            /// <summary>
            /// Indicate that the sensor is either inactive or a free-fall condition
            /// has been detected.
            /// </summary>
            public const byte InactivityDetected = 0x20;

            /// <summary>
            /// Indicate if the sensor is awake (true) or inactive (false).
            /// </summary>
            public const byte Awake = 0x40;

            /// <summary>
            /// SEU Error Detect. 1 indicates one of two conditions: either an
            /// SEU event, such as an alpha particle of a power glitch, has disturbed
            /// a user register setting or the ADXL362 is not configured. This bit
            /// is high upon both startup and soft reset, and resets as soon as any
            /// register write commands are performed.
            /// </summary>
            public const byte ErrorUserRegister = 0x80;
        }
    }
}
