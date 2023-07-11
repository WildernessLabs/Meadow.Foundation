namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Sc16is7x2
    {
        /// <summary>
        /// The list of possible I2C addresses for the peripheral
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x48
            /// </summary>
            Address_0x48 = 0x48,
            /// <summary>
            /// Bus address 0x49
            /// </summary>
            Address_0x49 = 0x49,
            /// <summary>
            /// Bus address 0x4A
            /// </summary>
            Address_0x4A = 0x4A,
            /// <summary>
            /// Bus address 0x4B
            /// </summary>
            Address_0x4B = 0x4B,
            /// <summary>
            /// Bus address 0x4C
            /// </summary>
            Address_0x4C = 0x4C,
            /// <summary>
            /// Bus address 0x4D
            /// </summary>
            Address_0x4D = 0x4D,
            /// <summary>
            /// Bus address 0x4E
            /// </summary>
            Address_0x4E = 0x4E,
            /// <summary>
            /// Bus address 0x4F
            /// </summary>
            Address_0x4F = 0x4F,
            /// <summary>
            /// Bus address 0x50
            /// </summary>
            Address_0x50 = 0x50,
            /// <summary>
            /// Bus address 0x51
            /// </summary>
            Address_0x51 = 0x51,
            /// <summary>
            /// Bus address 0x52
            /// </summary>
            Address_0x52 = 0x52,
            /// <summary>
            /// Bus address 0x53
            /// </summary>
            Address_0x53 = 0x53,
            /// <summary>
            /// Bus address 0x54
            /// </summary>
            Address_0x54 = 0x54,
            /// <summary>
            /// Bus address 0x55
            /// </summary>
            Address_0x55 = 0x55,
            /// <summary>
            /// Bus address 0x56
            /// </summary>
            Address_0x56 = 0x56,
            /// <summary>
            /// Bus address 0x57
            /// </summary>
            Address_0x57 = 0x57,
            /// <summary>
            /// The default bus address (0x48)
            /// </summary>
            Default = Address_0x48,
        }

        internal enum Registers : byte
        {
            // general registers
            RHR = 0x00,
            THR = 0x00,
            IER = 0x01,
            FCR = 0x02,
            IIR = 0x02,
            LCR = 0x03,
            MCR = 0x04,
            LSR = 0x05,
            MSR = 0x06,
            TCR = 0x06,
            SPR = 0x07,
            TLR = 0x07,
            TXLVL = 0x08,
            RXLVL = 0x09,
            IODir = 0x0A,
            IOState = 0x0B,
            IOIntEna = 0x0C,
            IOControl = 0x0E,
            EFCR = 0x0F,

            // special registers
            DLL = 0x00,
            DLH = 0x01,

            // enhanced registers
            EFR = 0x02,
            XON1 = 0x04,
            XON2 = 0x05,
            XOFF1 = 0x06,
            XOFF2 = 0x07
        }

        internal enum Channels
        {
            Both = 0,
            A = 0,
            B = 1
        }

        internal static class RegisterBits
        {
            public const byte IOCTL_RESET = 1 << 3;

            public const byte FCR_FIFO_ENABLE = 1 << 0;
            public const byte FCR_RX_FIFO_RESET = 1 << 1;
            public const byte FCR_TX_FIFO_RESET = 1 << 2;

            public const byte LCR_5_DATA_BITS = 0;
            public const byte LCR_6_DATA_BITS = 1;
            public const byte LCR_7_DATA_BITS = 2;
            public const byte LCR_8_DATA_BITS = 3;
            public const byte LCR_2_STOP_BITS = 1 << 2;
            public const byte LCR_PARITY_NONE = 0;
            public const byte LCR_PARITY_ODD = 1 << 3; // bit 3
            public const byte LCR_PARITY_EVEN = 3 << 3; // bits 3 and 4
            public const byte LCR_DIVISOR_LATCH_ENABLE = 1 << 7;

            public const byte LSR_THR_EMPTY = 1 << 5;

            public const byte IIR_RHR_INTERRUPT = 1 << 2;

            public const byte IER_RHR_ENABLE = 1 << 0;
            public const byte IER_THR_ENABLE = 1 << 1;
            public const byte IER_LINE_STATUS_ENABLE = 1 << 2;
            public const byte IER_SLEEP_MODE_ENABLE = 1 << 4;

            public const byte MCR_DTR = 1 << 0;
            public const byte MCR_RTS = 1 << 2;
            public const byte MCR_CLOCK_DIVISOR = 1 << 7;

            public const byte EFCR_9BITMODE = 1 << 0;
            public const byte EFCR_RTSCON = 1 << 4;
            public const byte EFCR_RTSINVER = 1 << 5;


        }
    }
}