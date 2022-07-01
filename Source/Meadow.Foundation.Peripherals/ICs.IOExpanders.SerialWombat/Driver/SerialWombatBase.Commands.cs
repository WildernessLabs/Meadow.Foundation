namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        internal static class Commands
        {
            public static readonly byte[] GetVersion = new byte[] { (byte)'V', 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 };
            public static readonly byte[] ReadPublicData = new byte[] { 0x81, 0x00, 0xff, 0xff, 0x55, 0x55, 0x55, 0x55 }; // byte[1] == pin, must be replaced at command time
            public static readonly byte[] WritePublicData = new byte[] { 0x82, 0x00, 0x00, 0x00, 0xff, 0x55, 0x55, 0x55 }; // byte[1] == pin, 2-3 == value. must be replaced at command time
            public static readonly byte[] ReadFlash = new byte[] { 0xA1, 0x00, 0x00, 0x00, 0x00, 0x55, 0x55, 0x55 }; // byte[2-4] == address. must be replaced at command time
            public static readonly byte[] SetPinMode = new byte[] { 0xC8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x55 }; // byte[1-6] == pin mode declarations. must be replaced at command time
        }
    }
}