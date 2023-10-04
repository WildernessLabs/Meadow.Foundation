using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an Il3897 ePaper display
    /// </summary>
    public class Il3897 : EPaperMonoBase
    {
        /// <summary>
        /// Create a new Il3897 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il3897(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width = 122, int height = 250) :
            base(spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

        /// <summary>
        /// Create a new Il3897 ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il3897(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort,
            int width, int height) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, width, height)
        { }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            Reset();

            SendCommand(Command.DRIVER_OUTPUT_CONTROL);
            SendData(Height - 1);
            SendData((Height - 1) >> 8);
            SendData(0x00);                     // GD = 0; SM = 0; TB = 0;

            SendCommand(Command.BOOSTER_SOFT_START_CONTROL);
            SendData(0xD7);
            SendData(0xD6);
            SendData(0x9D);

            SendCommand(Command.WRITE_VCOM_REGISTER);
            SendData(0xA8);                     // VCOM 7C

            SendCommand(Command.SET_DUMMY_LINE_PERIOD);
            SendData(0x1A);                     // 4 dummy lines per gate

            SendCommand(Command.SET_GATE_TIME);
            SendData(0x08);                     // 2us per line

            SendCommand(Command.DATA_ENTRY_MODE_SETTING);
            SendData(0x03);                     // X increment; Y increment

            SendData(LUT_Full_Update);
        }

        static readonly byte[] LUT_Full_Update =
        {
            0x22, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x11,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        /*
        static readonly byte[] LUT_Partial_Update =
        {
            0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0F, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        */
    }
}