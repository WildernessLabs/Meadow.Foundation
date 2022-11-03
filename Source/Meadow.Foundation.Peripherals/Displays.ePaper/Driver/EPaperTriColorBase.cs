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
        /// The color to draw when a pixel is enabled
        /// </summary>
        public Color EnabledColor => Color.Black;

        /// <summary>
        /// The color to draw when a pixel is disabled
        /// </summary>
        public Color DisabledColor => Color.White;

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

        protected Buffer2bppEPaper imageBuffer;

        /// <summary>
        /// Width of display in pixels
        /// </summary>
        public virtual int Width => width;

        /// <summary>
        /// Height of display in pixels
        /// </summary>
        public virtual int Height => height;

        /// <summary>
        /// The pixel buffer - not directly accessible
        /// Use buffer.BlackBuffer and buffer.ColorBuffer to access byte arrays
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        int width, height;

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

            this.width = width;
            this.height = height;

            int bufferWidth = width % 8 > 0 ? width + 8 - (width % 8) : width;

            CreateBuffer(bufferWidth, height);
            imageBuffer.Clear();

            Initialize();
        }

        /// <summary>
        /// Create an offscreen buffer for the display
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        protected virtual void CreateBuffer(int width, int height)
        {
            imageBuffer = new Buffer2bppEPaper(width, height);
        }

        /// <summary>
        /// Initalize the display
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Clear the display buffer 
        /// </summary>
        /// <param name="updateDisplay">Update the display if true</param>
        public void Clear(bool updateDisplay = false)
        {
            Clear(false, updateDisplay);
        }

        /// <summary>
        /// Fill the display buffer with a color
        /// </summary>
        /// <param name="color">The color - normalized to black, white or color</param>
        /// <param name="updateDisplay">Refresh the display if true</param>
        public void Fill(Color color, bool updateDisplay = false)
        {
            bool enabled = false;
            if (color.B > 0 || color.R > 0 || color.G > 0)
                enabled = true;

            Clear(enabled, updateDisplay);
        }

        /// <summary>
        /// Fill a region of the display buffer with a color
        /// </summary>
        /// <param name="x">The x location</param>
        /// <param name="y">The y location</param>
        /// <param name="width">The width to fill in pixels</param>
        /// <param name="height">The height to fill in pixels</param>
        /// <param name="color">The color to fill - normalized to black, white or color</param>
        public void Fill(int x, int y, int width, int height, Color color)
        {
            imageBuffer.Fill(color);
        }

        /// <summary>
        /// Clear the display buffer
        /// </summary>
        /// <param name="enabled">If true, fill with the enabled color (default is white)</param>
        /// <param name="updateDisplay">If true, refresh the display</param>
        public void Clear(bool enabled, bool updateDisplay = false)
        {
            imageBuffer.Fill(enabled ? Color.Black : Color.White);

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Draw a pixel
        /// </summary>
        /// <param name="x">The x location in pixels</param>
        /// <param name="y">The y location in pixels</param>
        /// <param name="enabled">If true, use the enabled color (default is white)</param>
        public void DrawPixel(int x, int y, bool enabled) => DrawBlackPixel(x, y, enabled);

        /// <summary>
        /// Draw a black pixel
        /// </summary>
        /// <param name="x">The x location in pixels</param>
        /// <param name="y">The y location in pixels</param>
        /// <param name="enabled">If true, use the enabled color (default is white)</param>
        public void DrawBlackPixel(int x, int y, bool enabled)
        {
            if(IsBlackInverted) { enabled = !enabled; }

            imageBuffer.SetBlackPixel(x, y, enabled);
        }

        /// <summary>
        /// Invert a pixel 
        /// </summary>
        /// <param name="x">The x location in pixels</param>
        /// <param name="y">The y location in pixels</param>
        public void InvertPixel(int x, int y)
        {
            imageBuffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Set a colored pixel (on or off)
        /// </summary>
        /// <param name="x">The x pixel location</param>
        /// <param name="y">The y pixel location</param>
        /// <param name="isOn">True for on, false for off</param>
        public void DrawColoredPixel(int x, int y, bool isOn)
        {
            imageBuffer.SetColorPixel(x, y, isOn);
        }

        /// <summary>
        /// Draw a pixel
        /// </summary>
        /// <param name="x">The x pixel location</param>
        /// <param name="y">The y pixel location</param>
        /// <param name="color">The pixel color</param>
        public void DrawPixel(int x, int y, Color color)
        {
            imageBuffer.SetPixel(x, y, color);
        }

        /// <summary>
        /// Write a buffer to the display buffer
        /// </summary>
        /// <param name="x">The x position in pixels to write the buffer</param>
        /// <param name="y">The y position in pixels to write the buffer</param>
        /// <param name="displayBuffer">The buffer to write</param>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">The command</param>
        internal void SendCommand(Command command)
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

        /// <summary>
        /// Update a region of the display from the offscreen buffer
        /// </summary>
        /// <param name="left">Left bounds in pixels</param>
        /// <param name="top">Top bounds in pixels</param>
        /// <param name="right">Right bounds in pixels</param>
        /// <param name="bottom">Bottom bounds in pixels</param>
        public virtual void Show(int left, int top, int right, int bottom)
        {
            throw new NotImplementedException("Show must be implimented in the ePaper display driver");
        }
        
        /// <summary>
        /// Display commands
        /// </summary>
        internal enum Command : byte
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