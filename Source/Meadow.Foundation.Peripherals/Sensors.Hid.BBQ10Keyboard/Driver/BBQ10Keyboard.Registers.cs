namespace Meadow.Foundation.Sensors.Hid
{
    public partial class BBQ10Keyboard
    {
        enum Registers : byte
        {
            VER = 0x01, // fw version
            CFG = 0x02, // config
            INT = 0x03, // interrupt status
            KEY = 0x04, // key status
            BKL = 0x05, // backlight
            DEB = 0x06, // debounce cfg
            FRQ = 0x07, // poll freq cfg
            RST = 0x08, // reset
            FIF = 0x09, // fifo
            BK2 = 0x0A, // backlight 2
            DIR = 0x0B, // gpio direction
            PUE = 0x0C, // gpio input pull enable
            PUD = 0x0D, // gpio input pull direction
            GIO = 0x0E, // gpio value
            GIC = 0x0F, // gpio interrupt config
            GIN = 0x10 // gpio interrupt status
        }

        const byte WRITE_MASK = (1 << 7);
        const byte CFG_OVERFLOW_ON = (1 << 0);
        const byte CFG_OVERFLOW_INT = (1 << 1);
        const byte CFG_CAPSLOCK_INT = (1 << 2);
        const byte CFG_NUMLOCK_INT = (1 << 3);
        const byte CFG_KEY_INT = (1 << 4);
        const byte CFG_PANIC_INT = (1 << 5);

        const byte INT_OVERFLOW = (1 << 0);
        const byte INT_CAPSLOCK = (1 << 1);
        const byte INT_NUMLOCK = (1 << 2);
        const byte INT_KEY = (1 << 3);
        const byte INT_PANIC = (1 << 4);

        const byte KEY_CAPSLOCK = (1 << 5);
        const byte KEY_NUMLOCK = (1 << 6);
        const byte KEY_COUNT_MASK = (0x1F);

        const byte DIR_OUTPUT = 0;
        const byte DIR_INPUT = 1;


        const byte PUD_DOWN = 0;
        const byte PUD_UP = 1;
    }
}
