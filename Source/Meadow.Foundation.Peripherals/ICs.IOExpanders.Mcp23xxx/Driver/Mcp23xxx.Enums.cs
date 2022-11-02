namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x20
            /// </summary>
            Address_0x20 = 0x20,
            /// <summary>
            /// Bus address 0x21
            /// </summary>
            Address_0x21 = 0x21,
            /// <summary>
            /// Bus address 0x22
            /// </summary>
            Address_0x22 = 0x22,
            /// <summary>
            /// Bus address 0x23
            /// </summary>
            Address_0x23 = 0x23,
            /// <summary>
            /// Bus address 0x24
            /// </summary>
            Address_0x24 = 0x24,
            /// <summary>
            /// Bus address 0x25
            /// </summary>
            Address_0x25 = 0x25,
            /// <summary>
            /// Bus address 0x26
            /// </summary>
            Address_0x26 = 0x26,
            /// <summary>
            /// Bus address 0x27
            /// </summary>
            Address_0x27 = 0x27,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x20
        }

        /// <summary>
        /// The MCP28XXX family has an address mapping concept for accessing registers.
        /// This provides a way to easily address registers by group or type. This is only
        /// relevant for 16-bit devices where it has two banks (Port A and B) of 8-bit
        /// GPIO pins.
        /// </summary>
        public enum PortBankType : byte
        {
            /// <summary>
            /// This mode is used by 16-bit devices - it treats the two 8-bit banks as one 16-bit bank
            /// </summary>
            /// <remarks>
            /// Each of the registers are interleaved so that sending two bytes in a
            /// row will set the equivalent register for the second bank. This way you
            /// can set all 16 GPIO pins/settings with one command sequence.
            ///
            /// Note that this behavior is also dependent on the default behavior
            /// of IOCON.SEQOP = 0 (the default) which automatically increments the
            /// register address as bytes come in.
            ///
            /// This is IOCON.BANK = 0 and is the default.
            /// </remarks>
            Segregated = 0,

            /// <summary>
            /// This mode keeps the two 8-bit banks registers separate
            /// </summary>
            /// <remarks>
            /// While this keeps the register addresses for bank A the same as the
            /// 8-bit controllers it requires sending a separate command sequence to
            /// set all 16-bits as the second bank's register addresses are not
            /// sequential.
            ///
            /// Changing IOCON.SEQOP to 1 (not the default) will cause the
            /// register address pointer to toggle between Port A and B for the
            /// given register if in this mode.
            ///
            /// This is IOCON.BANK = 1
            /// </remarks>
            Paired = 1
        }
        
        /// <summary>
        /// The I/O port bank for 16 pin devices
        /// </summary>
        /// <remarks>
        /// 16-bit controllers are logically separated into two 8-bit ports. 8-bit
        /// controllers only have one "port" of GPIO pins and ingore the bank setting
        /// </remarks>
        public enum PortBank
        {
            /// <summary>
            /// The first set / bank of 8 GPIO pins
            /// </summary>
            A,
            /// <summary>
            /// The second set /bank of 8 GPIO pins (Mcp23x1x only)
            /// </summary>
            B
        }
    }
}