namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        internal enum Command : byte
        {
            READ_PIN_BUFFFER = 0x81, ///< (0x81)
            SET_PIN_BUFFFER = 0x82, ///< (0x82)  
            CONFIGURE_PIN_MODE0 = 200, ///< (200)
            CONFIGURE_PIN_MODE1 = 201, ///< (201)
            CONFIGURE_PIN_MODE2 = 202, ///< (202)
            CONFIGURE_PIN_MODE3 = 203, ///< (203)
            CONFIGURE_PIN_MODE4 = 204, ///< (204)
            CONFIGURE_PIN_MODE5 = 205, ///< (205)
            CONFIGURE_PIN_MODE6 = 206, ///< (206)
            CONFIGURE_PIN_MODE7 = 207, ///< (207)
            CONFIGURE_PIN_MODE8 = 208, ///< (208)
            CONFIGURE_PIN_MODE9 = 209, ///< (209)
            CONFIGURE_PIN_MODE10 = 210, ///< (210)
            CONFIGURE_PIN_OUTPUTSCALE = 210, ///< (210)
            CONFIGURE_PIN_INPUTPROCESS = 211, ///< (211)
            CONFIGURE_PIN_MODE_HW_0 = 220, ///< (220)
        }

        internal static class Commands
        {
            public static readonly byte[] GetVersion = new byte[] { (byte)'V', 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 };
            public static readonly byte[] ReadPublicData = new byte[] { 0x81, 0x00, 0xff, 0xff, 0x55, 0x55, 0x55, 0x55 }; // byte[1] == pin, must be replaced at command time
            public static readonly byte[] WritePublicData = new byte[] { 0x82, 0x00, 0x00, 0x00, 0xff, 0x55, 0x55, 0x55 }; // byte[1] == pin, 2-3 == value. must be replaced at command time
            public static readonly byte[] ReadFlash = new byte[] { 0xA1, 0x00, 0x00, 0x00, 0x00, 0x55, 0x55, 0x55 }; // byte[2-4] == address. must be replaced at command time
            public static readonly byte[] SetPinMode0 = new byte[] { (byte)Command.CONFIGURE_PIN_MODE0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x55 }; // byte[1-6] == pin mode declarations. must be replaced at command time
            public static readonly byte[] SetPinModeHW0 = new byte[] { (byte)Command.CONFIGURE_PIN_MODE_HW_0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x55 };
        }
    }
}