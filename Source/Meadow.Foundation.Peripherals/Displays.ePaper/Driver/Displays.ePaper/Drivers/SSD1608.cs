using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    //WaveShare 1.54" BW
    public class SSD1608 : EPDBase
    {
        public SSD1608(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            uint width = 200, uint height = 200) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        {
        }

        protected override void Initialize()
        {
            Reset();

            SendCommand(DRIVER_OUTPUT_CONTROL);
            SendData((byte)(Height - 1));
            SendData((int)(Height - 1) >> 8);
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
            0x02, 0x02, 0x01, 0x11, 0x12, 0x12, 0x22, 0x22,
            0x66, 0x69, 0x69, 0x59, 0x58, 0x99, 0x99, 0x88,
            0x00, 0x00, 0x00, 0x00, 0xF8, 0xB4, 0x13, 0x51,
            0x35, 0x51, 0x51, 0x19, 0x01, 0x00
        };

        public static readonly byte[] LUT_Partial_Update =
        {
            0x10, 0x18, 0x18, 0x08, 0x18, 0x18, 0x08, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x13, 0x14, 0x44, 0x12,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
    }
}