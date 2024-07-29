
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
            DEVICE_ID = 0x00,
            TAP_THRESHOLD = 0x1D,
            OFFSET_X = 0x1E,
            OFFSET_Y = 0x1F,
            OFFSET_Z = 0x20,
            TAP_DURATION = 0x21,
            TAP_LATENCY = 0x22,
            TAP_WINDOW = 0x23,
            ACTIVITY_THRESHOLD = 0x24,
            INACTIVITY_THRESHOLD = 0x25,
            INACTIVITY_TIME = 0x26,
            ACTIVITY_INACTIVITY_CONTROL = 0x27,
            FREEFALL_THRESHOLD = 0x28,
            FREEFALL_TIME = 0x29,
            TAP_AXES = 0x2A,
            TAP_ACTIVITY_STATUS = 0x2B,
            DATA_RATE = 0x2C,
            POWER_CONTROL = 0x2D,
            INTERRUPT_ENABLE = 0x2E,
            INTERRUPT_MAP = 0x2F,
            INTERRUPT_SOURCE = 0x30,
            DATA_FORMAT = 0x31,
            DATAX0 = 0x32,
            DATAX1 = 0x33,
            DATAY0 = 0x33,
            DATAY1 = 0x34,
            DATAZ0 = 0x36,
            DATAZ1 = 0x37,
            FIFO_CONTROL = 0x38,
            FIFO_STATUS = 0x39,
        }
    }
}