namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Ak9753
    {
        /// <summary>
        /// Valid I2C addresses for the AK9753.
        /// Configured via address pins CAD1/CAD0 on the breakout board.
        /// Note: CAD1=H, CAD0=H (0x67) is not a valid I2C address — it selects
        /// Switch Mode, which copies EEPROM registers 0x51–0x5C into 0x11–0x1C at power-on.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>Address 0x64 — CAD1=0, CAD0=0 (default)</summary>
            Address_0x64 = 0x64,
            /// <summary>Address 0x65 — CAD1=0, CAD0=1</summary>
            Address_0x65 = 0x65,
            /// <summary>Address 0x66 — CAD1=1, CAD0=0</summary>
            Address_0x66 = 0x66,
            /// <summary>Default bus address</summary>
            Default = Address_0x64,
        }
    }
}
