using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    //EPD2i13
    public class IL3897 : EPDBase
    {
        public IL3897(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            uint width = 122, uint height = 250) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

        protected override void Initialize()
        {
            Reset();

            SendCommand(DRIVER_OUTPUT_CONTROL);
            SendData((int)(Width - 1));
            SendData((int)(Width - 1) >> 8);
            SendData(0x00);                     // GD = 0; SM = 0; TB = 0;

            SendCommand(BOOSTER_SOFT_START_CONTROL);
            SendData(0xD7);
            SendData(0xD6);
            SendData(0x9D);

            SendCommand(WRITE_VCOM_REGISTER);
            SendData(0xA8);                     // VCOM 7C

            SendCommand(SET_DUMMY_LINE_PERIOD);
            SendData(0x1A);                     // 4 dummy lines per gate

            SendCommand(SET_GATE_TIME);
            SendData(0x08);                     // 2us per line

            SendCommand(DATA_ENTRY_MODE_SETTING);
            SendData(0x03);                     // X increment; Y increment

            SendData(LUT_Full_Update);
        }

        public static readonly byte[] LUT_Full_Update =
        {
            0x22, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x11,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        public static readonly byte[] LUT_Partial_Update =
        {
            0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0F, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
    }
}
