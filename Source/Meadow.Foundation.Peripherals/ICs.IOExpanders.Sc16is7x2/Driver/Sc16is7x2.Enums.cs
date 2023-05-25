namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Sc16is7x2
    {
        public enum Addresses : byte
        {
            Address_0x48 = 0x48,
            Address_0x49 = 0x49,
            Address_0x4A = 0x4A,
            Address_0x4B = 0x4B,
            Address_0x4C = 0x4C,
            Address_0x4D = 0x4D,
            Address_0x4E = 0x4E,
            Address_0x4F = 0x4F,
            Address_0x50 = 0x50,
            Address_0x51 = 0x51,
            Address_0x52 = 0x52,
            Address_0x53 = 0x53,
            Address_0x54 = 0x54,
            Address_0x55 = 0x55,
            Address_0x56 = 0x56,
            Address_0x57 = 0x57,
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

            public const byte MCR_DTR = 1 << 0;
            public const byte MCR_RTS = 1 << 2;
            public const byte MCR_CLOCK_DIVISOR = 1 << 7;

            public const byte EFCR_9BITMODE = 1 << 0;
            public const byte EFCR_RTSCON = 1 << 4;
            public const byte EFCR_RTSINVER = 1 << 5;


        }
    }
}