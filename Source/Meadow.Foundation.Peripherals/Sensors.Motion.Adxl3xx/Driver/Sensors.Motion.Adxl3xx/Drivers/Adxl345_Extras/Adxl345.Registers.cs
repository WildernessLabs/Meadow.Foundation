using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl345
    {
        /// <summary>
        /// Control registers for the ADXL345 chip.
        /// </summary>
        /// <remarks>
        /// Taken from table 19 on page 23 of the data sheet.
        /// </remarks>
        private static class Registers
        {
            public const byte ACTIVITY_INACTIVITY_CONTROL = 0x27;
            public const byte ACTIVITY_THRESHOLD = 0x24;
            public const byte DATA_FORMAT = 0x31;
            public const byte DATA_RATE = 0x2c;
            public const byte DEVICE_ID = 0x00;
            public const byte FIFO_CONTROL = 0x38;
            public const byte FIFO_STATUS = 0x39;
            public const byte FREEFALL_THRESHOLD = 0x28;
            public const byte FREEFALL_TIME = 0x29;
            public const byte INACTIVITY_THRESHOLD = 0x25;
            public const byte INACTIVITY_TIME = 0x26;
            public const byte INTERRUPT_ENABLE = 0x2e;
            public const byte INTERRUPT_MAP = 0x2f;
            public const byte INTERRUPT_SOURCE = 0x30;
            public const byte OFFSET_X = 0x1e;
            public const byte OFFSET_Y = 0x1f;
            public const byte OFFSET_Z = 0x20;
            public const byte POWER_CONTROL = 0x2d;
            public const byte TAP_ACTIVITY_STATUS = 0x2a;
            public const byte TAP_AXES = 0x2a;
            public const byte TAP_DURATION = 0x21;
            public const byte TAP_LATENCY = 0x22;
            public const byte TAP_THRESHOLD = 0x1d;
            public const byte TAP_WINDOW = 0x23;
            public const byte X0 = 0x32;
            public const byte X1 = 0x33;
            public const byte Y0 = 0x33;
            public const byte Y1 = 0x34;
            public const byte Z0 = 0x36;
            public const byte Z1 = 0x37;
        }
    }
}
