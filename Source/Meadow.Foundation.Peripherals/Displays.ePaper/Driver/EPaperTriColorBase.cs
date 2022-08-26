using System;
using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface for ePaper 3 color displays
    /// </summary>
    public abstract partial class EPaperTriColorBase : EPaperBase, IGraphicsDisplay
    {
        /// <summary>
        /// Is the black pixel data inverted
        /// </summary>
        protected abstract bool IsBlackInverted { get; }

        /// <summary>
        /// Is the color pixel data inverted
        /// </summary>
        protected abstract bool IsColorInverted { get; }

        /// <summary>
        /// Display color mode 
        /// </summary>
        public ColorType ColorMode => ColorType.Format2bpp;

        /// <summary>
        /// The buffer the holds the black pixel data for the display
        /// </summary>

        protected readonly Buffer2bppEPaper imageBuffer;

        /// <summary>
        /// Width of display in pixels
        /// </summary>
        public int Width => imageBuffer.Width;

        /// <summary>
        /// Height of display in pixels
        /// </summary>
        public int Height => imageBuffer.Height;

        /// <summary>
        /// The pixel buffer - not directly accessible
        /// Use buffer.BlackBuffer and buffer.ColorBuffer to access byte arrays
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// Create a new color ePaper display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public EPaperTriColorBase(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            this(spiBus, device.CreateDigitalOutputPort(chipSelectPin), device.CreateDigitalOutputPort(dcPin, false),
                device.CreateDigitalOutputPort(resetPin, true), device.CreateDigitalInputPort(busyPin),
                width, height)
        { }

        /// <summary>
        /// Create a new ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public EPaperTriColorBase(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort,
            int width, int height)
        {
            this.dataCommandPort = dataCommandPort;
            this.chipSelectPort = chipSelectPort;
            this.resetPort = resetPort;
            this.busyPort = busyPort;

            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            imageBuffer = new Buffer2bppEPaper(width, height);
            imageBuffer.Clear();

            Initialize();
        }

        protected abstract void Initialize();

        /// <summary>
        /// Clear the display buffer 
        /// </summary>
        /// <param name="updateDisplay">Update the display if true</param>
        public void Clear(bool updateDisplay = false)
        {
            Clear(false, updateDisplay);
        }

        public void Fill(Color color, bool updateDisplay = false)
        {
            bool colored = false;
            if (color.B > 0 || color.R > 0 || color.G > 0)
                colored = true;

            Clear(colored, updateDisplay);
        }

        public void Fill(int x, int y, int width, int height, Color color)
        {
            imageBuffer.Fill(color);
        }

        public void Clear(bool colored, bool updateDisplay = false)
        {
            imageBuffer.Fill(colored?Color.Black : Color.White);

            if (updateDisplay)
            {
                Show();
            }
        }

        public void DrawPixel(int x, int y, bool isOn) => DrawBlackPixel(x, y, isOn);

        public void DrawBlackPixel(int x, int y, bool isOn)
        {
            if(IsBlackInverted) { isOn = !isOn; }

            imageBuffer.SetBlackPixel(x, y, isOn);
        }

        public void InvertPixel(int x, int y)
        {
            imageBuffer.InvertPixel(x, y);
        }

        public void DrawColoredPixel(int x, int y, bool isOn)
        {
            imageBuffer.SetColorPixel(x, y, isOn);
        }

        public void DrawPixel(int x, int y, Color color)
        {
            imageBuffer.SetPixel(x, y, color);
        }

        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">The command</param>
        protected void SendCommand(Command command)
        {
            SendCommand((byte)command);
        }

        /// <summary>
        /// Update the display
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Show()
        {
            throw new NotImplementedException("Show must be implimented in the ePaper display driver");
        }

        public virtual void Show(int left, int top, int right, int bottom)
        {
            throw new NotImplementedException("Show must be implimented in the ePaper display driver");
        }
        
        // 2.13b + 2.7b (red) commands

        protected enum Command : byte
        {
            PANEL_SETTING = 0x00,
            POWER_SETTING = 0x01,
            POWER_OFF = 0x02,
            POWER_OFF_SEQUENCE_SETTING = 0x03,
            POWER_ON = 0x04,
            POWER_ON_MEASURE = 0x05,
            BOOSTER_SOFT_START = 0x06,
            DEEP_SLEEP = 0x07,
            DATA_START_TRANSMISSION_1 = 0x10,
            DATA_STOP = 0x11,
            DISPLAY_REFRESH = 0x12,
            DATA_START_TRANSMISSION_2 = 0x13,
            PARTIAL_DATA_START_TRANSMISSION_1 = 0x14,
            PARTIAL_DATA_START_TRANSMISSION_2 = 0x15,
            PARTIAL_DISPLAY_REFRESH = 0x16,
            LUT_FOR_VCOM = 0x20,
            LUT_WHITE_TO_WHITE = 0x21,
            LUT_BLACK_TO_WHITE = 0x22,
            LUT_WHITE_TO_BLACK = 0x23,
            LUT_BLACK_TO_BLACK = 0x24,
            PLL_CONTROL = 0x30,
            TEMPERATURE_SENSOR_CALIBRATION = 0x40,
            TEMPERATURE_SENSOR_SELECTION = 0x41,
            TEMPERATURE_SENSOR_WRITE = 0x42,
            TEMPERATURE_SENSOR_READ = 0x43,
            VCOM_AND_DATA_INTERVAL_SETTING = 0x50,
            LOW_POWER_DETECTION = 0x51,
            TCON_SETTING = 0x60,
            RESOLUTION_SETTING = 0x61,
            SOURCE_AND_GATE_START_SETTING = 0x62,
            GET_STATUS = 0x71,
            AUTO_MEASURE_VCOM = 0x80,
            READ_VCOM_VALUE = 0x81,
            VCM_DC_SETTING = 0x82,
            PARTIAL_WINDOW = 0x90,
            PARTIAL_IN = 0x91,
            PARTIAL_OUT = 0x92,
            PROGRAM_MODE = 0xA0,
            ACTIVE_PROGRAM = 0xA1,
            READ_OTP_DATA = 0xA2,
            POWER_SAVING = 0xE3,
        }
    }
}