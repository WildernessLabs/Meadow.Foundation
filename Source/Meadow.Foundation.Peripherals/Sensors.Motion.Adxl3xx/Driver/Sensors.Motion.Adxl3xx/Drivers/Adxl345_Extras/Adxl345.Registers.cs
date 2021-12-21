
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
        private enum Register : byte
        {
            ACTIVITY_INACTIVITY_CONTROL = 0x27,
            ACTIVITY_THRESHOLD = 0x24,
            DATA_FORMAT = 0x31,
            DATA_RATE = 0x2c,
            DEVICE_ID = 0x00,
            FIFO_CONTROL = 0x38,
            FIFO_STATUS = 0x39,
            FREEFALL_THRESHOLD = 0x28,
            FREEFALL_TIME = 0x29,
            INACTIVITY_THRESHOLD = 0x25,
            INACTIVITY_TIME = 0x26,
            INTERRUPT_ENABLE = 0x2e,
            INTERRUPT_MAP = 0x2f,
            INTERRUPT_SOURCE = 0x30,
            OFFSET_X = 0x1e,
            OFFSET_Y = 0x1f,
            OFFSET_Z = 0x20,
            POWER_CONTROL = 0x2d,
            TAP_ACTIVITY_STATUS = 0x2a,
            TAP_AXES = 0x2a,
            TAP_DURATION = 0x21,
            TAP_LATENCY = 0x22,
            TAP_THRESHOLD = 0x1d,
            TAP_WINDOW = 0x23,
            X0 = 0x32,
            X1 = 0x33,
            Y0 = 0x33,
            Y1 = 0x34,
            Z0 = 0x36,
            Z1 = 0x37
        }
    }
}
