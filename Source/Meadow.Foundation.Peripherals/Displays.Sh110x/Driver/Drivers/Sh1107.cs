using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents the Sh1107 family of displays (up to 128x128)
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
        /// <param name="firstColumn">The first visible column on the display (if display is cropped)</param>
        public Sh1107(II2cBus i2cBus, byte address, int width = 128, int height = 128, int firstColumn = 0)
            : base(i2cBus, address, width, height, firstColumn)
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
        /// <param name="firstColumn">The first visible column on the display (if display is cropped)</param>
        public Sh1107(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, 
            int width = 128, int height = 128, int firstColumn = 0)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, firstColumn)
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
        /// <param name="firstColumn">The first visible column on the display (if display is cropped)</param>
        public Sh1107(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
            int width = 128, int height = 128, int firstColumn = 0)
            : base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, firstColumn)
        { }

        /// <summary>
        /// Initialize the Sh1107
        /// </summary>
        protected override void Initialize()
        {
            SendCommand(DisplayCommand.DisplayOff);

            SendCommand(DisplayCommand.ColumnAddressLow);
            SendCommand(DisplayCommand.ColumnAddressHigh);
            SendCommand(DisplayCommand.PageAddress);
            
            SendCommand(DisplayCommand.SetDisplayStartLine);
            SendCommand((byte)0x00);
            SendCommand(DisplayCommand.SetDisplayOffset);   
            SendCommand((byte)0x00);

            SendCommand(DisplayCommand.SetContrast);
            SendCommand(0x4F);

            SendCommand(DisplayCommand.PageAddressMode);
            SendCommand(DisplayCommand.SetSegmentNormal);
            SendCommand(DisplayCommand.ScanDirectionStandard);

            SendCommand(DisplayCommand.SetMultiplexRatio);
            SendCommand(0x7F);

            SendCommand(DisplayCommand.SetDisplayClockDiv);
            SendCommand(0x51);
            SendCommand(DisplayCommand.SetChargePeriods);
            SendCommand(0x22);
            SendCommand(DisplayCommand.SetVComDeselect);
            SendCommand(0x35);

            SendCommand(DisplayCommand.SetDCDCStatus);
            SendCommand(DisplayCommand.DCDCOff);
            SendCommand(DisplayCommand.DisplayResume);
            SendCommand(DisplayCommand.DisplayVideoNormal);
            Thread.Sleep(100);

            SendCommand(DisplayCommand.DisplayOn);
        }

        /// <inheritdoc/>
        public override void SetDisplayOffsets(byte startLine = 0, byte offset = 0)
        {
            SendCommand(DisplayCommand.SetDisplayStartLine);
            SendCommand(startLine);
            SendCommand(DisplayCommand.SetDisplayOffset);
            SendCommand(offset);
        }

    }
}
