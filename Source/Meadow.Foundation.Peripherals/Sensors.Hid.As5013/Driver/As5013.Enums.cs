namespace Meadow.Foundation.Sensors.Hid
{
    public partial class As5013
    {
        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x40
            /// </summary>
            Address_0x40 = 0x40,
            /// <summary>
            /// Bus address 0x41
            /// </summary>
            Address_0x41 = 0x41,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x40
        }

        enum Register
        {
            //    Manufacture ID code
            JOYSTICK_ID_CODE = 0x0C,
            //    Component ID version
            JOYSTICK_ID_VERSION = 0x0D,
            //    Silicon revision
            JOYSTICK_SIL_REV = 0x0E,
            //    Control register 1
            JOYSTICK_CONTROL1 = 0x0F,
            //    Control register 2
            JOYSTICK_CONTROL2 = 0x2E,
            //    Result x
            JOYSTICK_X = 0x10,
            //    Result y
            JOYSTICK_Y_RES_INT = 0x11,
            //    Positive X direction
            JOYSTICK_XP = 0x12,
            //    Negative X direction
            JOYSTICK_XN = 0x13,
            //    Positive Y direction
            JOYSTICK_YP = 0x14,
            //    Negative Y direction
            JOYSTICK_YN = 0x15,
            //    AGC
            JOYSTICK_AGC = 0x2A,
            //    Control register for the middle Hall element C5
            JOYSTICK_M_CTRL = 0x2B,
            //    Control register for the sector dependent attenuation of the outer Hall elements
            JOYSTICK_J_CTRL = 0x2C,
            //    Scale input to fit to the 8 bit result register
            JOYSTICK_T_CTRL = 0x2D,
        }

        enum Command : byte
        {
            // Required Test bits of Control 2 Register
            JOYSTICK_CONTROL2_TEST_CMD = 0x84,
            // Maximum sensitivity of Hall Element Direct Read Register
            JOYSTICK_AGC_MAX_SENSITIVITY_CMD = 0x3F,
            // Scaling Factor to 90.8% of T_ctrl Register
            JOYSTICK_T_CTRL_SCALING_90_8_CMD = 0x0A,
            // Scaling Factor to 90.8% of T_ctrl Register
            JOYSTICK_T_CTRL_SCALING_100_CMD = 0x09,
            // Reset Register of Control 1 Register
            JOYSTICK_CONTROL1_RESET_CMD = 0x88,
            // Invert the channel voltage
            JOYSTICK_INVERT_SPINING_CMD = 0x86,
        }
    }
}
