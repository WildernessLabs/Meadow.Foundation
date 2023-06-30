using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents the Sh1107 family of displays
    /// </summary>
    public class Sh1107 : Sh110x
    {
        /// <summary>
        /// Create a new Sh1107 object
        /// </summary>
        /// <param name="i2cBus">I2C bus connected to display</param>
        /// <param name="address">I2C address</param>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        public Sh1107(II2cBus i2cBus, byte address, int width = 128, int height = 128)
            : base(i2cBus, address, width, height)
        { }

        /// <summary>
        /// Create a new Sh1107 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        public Sh1107(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, int width = 128, int height = 128)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height)
        { }

        /// <summary>
        /// Create a new Sh1107 display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        public Sh1107(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            int width = 128, int height = 128)
            : base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height)
        { }

        /// <summary>
        /// Initialize the Sh1107
        /// </summary>
        protected override void Initialize()
        {
            SendCommand(DisplayCommand.DisplayOff);
            SendCommand(DisplayCommand.SetDisplayClockDiv);
            SendCommand(0x51);

            SendCommand(DisplayCommand.SetContrast);
            SendCommand(0x4F);

            SendCommand(DisplayCommand.DCDC);
            SendCommand(0x8A);

            SendCommand(DisplayCommand.SetSegmentRemap);
            SendCommand(DisplayCommand.ComScanInc);

            SendCommand(DisplayCommand.DisplayStartLine);
            SendCommand((byte)0);

            SendCommand(DisplayCommand.SegInvNormal);
            SendCommand(0xC0);

            SendCommand(DisplayCommand.SetPrecharge);
            SendCommand(0x22);

            SendCommand(DisplayCommand.SetVComDetect);
            SendCommand(0x35);

            SendCommand(DisplayCommand.DisplayOnResume);
            SendCommand(DisplayCommand.DisplayVideoNormal);

            if (Width == 128 && Height == 128)
            {
                SendCommand(DisplayCommand.SetDisplayOffset);
                SendCommand((byte)0x00);
                SendCommand(DisplayCommand.MultiplexModeSet);
                SendCommand(0x07F);
            }
            else
            {
                SendCommand(DisplayCommand.SetDisplayOffset);
                SendCommand(0x60);
                SendCommand(DisplayCommand.MultiplexModeSet);
                SendCommand(DisplayCommand.MultiplexDataSet);
            }

            Thread.Sleep(100);

            SendCommand(DisplayCommand.DisplayOn);
        }
    }
}