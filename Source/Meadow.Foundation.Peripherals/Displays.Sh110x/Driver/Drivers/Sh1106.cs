using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents the Sh1106 family of displays
    /// </summary>
    public class Sh1106 : Sh110x
    {
        /// <summary>
        /// Create a new Sh1106 object
        /// </summary>
        /// <param name="i2cBus">I2C bus connected to display</param>
        /// <param name="address">I2C address</param>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        public Sh1106(II2cBus i2cBus, byte address, int width, int height)
            : base(i2cBus, address, width, height)
        { }

        /// <summary>
        /// Create a new Sh1106 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        public Sh1106(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, int width = 128, int height = 64)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height)
        { }

        /// <summary>
        /// Create a new Sh1106 display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        public Sh1106(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            int width = 128, int height = 64)
            : base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height)
        { }

        /// <summary>
        /// Initialize the Sh1106
        /// </summary>
        protected override void Initialize()
        {
            Reset();

            SendCommand(DisplayCommand.DisplayOff);
            SendCommand(DisplayCommand.SetDisplayClockDiv);
            SendCommand(0x80);

            SendCommand(DisplayCommand.MultiplexModeSet);
            SendCommand(DisplayCommand.MultiplexDataSet);

            SendCommand(DisplayCommand.SetDisplayOffset);
            SendCommand((byte)0);

            SendCommand(DisplayCommand.DisplayStartLine);

            SendCommand(DisplayCommand.SegInvNormal);
            SendCommand(0xC0);

            SendCommand(DisplayCommand.SetComPins);
            SendCommand(0x12);

            SendCommand(DisplayCommand.SetContrast);
            SendCommand(0x0F);

            SendCommand(0x30);
            SendCommand(DisplayCommand.DisplayOnResume);

            SendCommand(DisplayCommand.SetDisplayClockDiv);
            SendCommand(0xF0);

            SendCommand(DisplayCommand.DisplayVideoNormal);

            Thread.Sleep(100);
            SendCommand(DisplayCommand.DisplayOn);
        }
    }
}