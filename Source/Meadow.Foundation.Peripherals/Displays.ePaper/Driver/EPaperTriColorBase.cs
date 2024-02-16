using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface for ePaper 3 color displays
    /// </summary>
    public abstract partial class EPaperTriColorBase : EPaperBase, IPixelDisplay
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

        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format2bpp;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format2bpp;

        /// <summary>
        /// The buffer the holds the black pixel data for the display
        /// </summary>

        protected Buffer2bppEPaper imageBuffer = default!;

        /// <inheritdoc/>
        public virtual int Width => width;

        /// <inheritdoc/>
        public virtual int Height => height;

        /// <summary>
        /// The pixel buffer - not directly accessible
        /// Use buffer.BlackBuffer and buffer.ColorBuffer to access byte arrays
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        readonly int width, height;

        /// <summary>
        /// Create a new color ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public EPaperTriColorBase(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            this(spiBus, chipSelectPin.CreateDigitalOutputPort(), dcPin.CreateDigitalOutputPort(false),
                resetPin.CreateDigitalOutputPort(true), busyPin.CreateDigitalInputPort(),
                width, height)
        {
            createdPorts = true;
        }

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

            spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

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
        /// Initialize the display
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
            if (IsBlackInverted) { enabled = !enabled; }

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
            throw new NotImplementedException("Show must be implemented in the ePaper display driver");
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
            throw new NotImplementedException("Show must be implemented in the ePaper display driver");
        }

        /// <summary>
        /// Display commands
        /// </summary>
        protected enum Command : byte
        {
            /// <summary>
            /// PANEL_SETTING
            /// </summary>
            PANEL_SETTING = 0x00,
            /// <summary>
            /// POWER_SETTING
            /// </summary>
            POWER_SETTING = 0x01,
            /// <summary>
            /// POWER_OFF
            /// </summary>
            POWER_OFF = 0x02,
            /// <summary>
            /// POWER_OFF_SEQUENCE_SETTING
            /// </summary>
            POWER_OFF_SEQUENCE_SETTING = 0x03,
            /// <summary>
            /// POWER_ON
            /// </summary>
            POWER_ON = 0x04,
            /// <summary>
            /// POWER_ON_MEASURE
            /// </summary>
            POWER_ON_MEASURE = 0x05,
            /// <summary>
            /// BOOSTER_SOFT_START
            /// </summary>
            BOOSTER_SOFT_START = 0x06,
            /// <summary>
            /// DEEP_SLEEP
            /// </summary>
            DEEP_SLEEP = 0x07,
            /// <summary>
            /// DATA_START_TRANSMISSION_1
            /// </summary>
            DATA_START_TRANSMISSION_1 = 0x10,
            /// <summary>
            /// DATA_STOP
            /// </summary>
            DATA_STOP = 0x11,
            /// <summary>
            /// DISPLAY_REFRESH
            /// </summary>
            DISPLAY_REFRESH = 0x12,
            /// <summary>
            /// DATA_START_TRANSMISSION_2
            /// </summary>
            DATA_START_TRANSMISSION_2 = 0x13,
            /// <summary>
            /// PARTIAL_DATA_START_TRANSMISSION_1
            /// </summary>
            PARTIAL_DATA_START_TRANSMISSION_1 = 0x14,
            /// <summary>
            /// PARTIAL_DATA_START_TRANSMISSION_2
            /// </summary>
            PARTIAL_DATA_START_TRANSMISSION_2 = 0x15,
            /// <summary>
            /// PARTIAL_DISPLAY_REFRESH
            /// </summary>
            PARTIAL_DISPLAY_REFRESH = 0x16,
            /// <summary>
            /// LUT_FOR_VCOM
            /// </summary>
            LUT_FOR_VCOM = 0x20,
            /// <summary>
            /// LUT_WHITE_TO_WHITE
            /// </summary>
            LUT_WHITE_TO_WHITE = 0x21,
            /// <summary>
            /// LUT_BLACK_TO_WHITE
            /// </summary>
            LUT_BLACK_TO_WHITE = 0x22,
            /// <summary>
            /// LUT_WHITE_TO_BLACK
            /// </summary>
            LUT_WHITE_TO_BLACK = 0x23,
            /// <summary>
            /// 
            /// </summary>
            LUT_BLACK_TO_BLACK = 0x24,
            /// <summary>
            /// PLL_CONTROL
            /// </summary>
            PLL_CONTROL = 0x30,
            /// <summary>
            /// TEMPERATURE_SENSOR_CALIBRATION
            /// </summary>
            TEMPERATURE_SENSOR_CALIBRATION = 0x40,
            /// <summary>
            /// TEMPERATURE_SENSOR_SELECTION
            /// </summary>
            TEMPERATURE_SENSOR_SELECTION = 0x41,
            /// <summary>
            /// TEMPERATURE_SENSOR_WRITE
            /// </summary>
            TEMPERATURE_SENSOR_WRITE = 0x42,
            /// <summary>
            /// TEMPERATURE_SENSOR_READ
            /// </summary>
            TEMPERATURE_SENSOR_READ = 0x43,
            /// <summary>
            /// VCOM_AND_DATA_INTERVAL_SETTING
            /// </summary>
            VCOM_AND_DATA_INTERVAL_SETTING = 0x50,
            /// <summary>
            /// LOW_POWER_DETECTION
            /// </summary>
            LOW_POWER_DETECTION = 0x51,
            /// <summary>
            /// TCON_SETTING
            /// </summary>
            TCON_SETTING = 0x60,
            /// <summary>
            /// RESOLUTION_SETTING
            /// </summary>
            RESOLUTION_SETTING = 0x61,
            /// <summary>
            /// SOURCE_AND_GATE_START_SETTING
            /// </summary>
            SOURCE_AND_GATE_START_SETTING = 0x62,
            /// <summary>
            /// GET_STATUS
            /// </summary>
            GET_STATUS = 0x71,
            /// <summary>
            /// AUTO_MEASURE_VCOM
            /// </summary>
            AUTO_MEASURE_VCOM = 0x80,
            /// <summary>
            /// READ_VCOM_VALUE
            /// </summary>
            READ_VCOM_VALUE = 0x81,
            /// <summary>
            /// VCM_DC_SETTING
            /// </summary>
            VCM_DC_SETTING = 0x82,
            /// <summary>
            /// PARTIAL_WINDOW
            /// </summary>
            PARTIAL_WINDOW = 0x90,
            /// <summary>
            /// PARTIAL_IN
            /// </summary>
            PARTIAL_IN = 0x91,
            /// <summary>
            /// PARTIAL_OUT
            /// </summary>
            PARTIAL_OUT = 0x92,
            /// <summary>
            /// PROGRAM_MODE
            /// </summary>
            PROGRAM_MODE = 0xA0,
            /// <summary>
            /// ACTIVE_PROGRAM
            /// </summary>
            ACTIVE_PROGRAM = 0xA1,
            /// <summary>
            /// READ_OTP_DATA
            /// </summary>
            READ_OTP_DATA = 0xA2,
            /// <summary>
            /// POWER_SAVING
            /// </summary>
            POWER_SAVING = 0xE3,
        }
    }
}