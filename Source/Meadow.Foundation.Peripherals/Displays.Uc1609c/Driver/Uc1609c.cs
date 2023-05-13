﻿using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Units;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a UC1609C single color LCD display
    /// </summary>
    public partial class Uc1609c : IGraphicsDisplay, ISpiPeripheral
    {
        /// <summary>
        /// The display color mode - 1 bit per pixel monochrome
        /// </summary>
        public ColorMode ColorMode => ColorMode.Format1bpp;

        /// <summary>
        /// The Color mode supported by the display
        /// </summary>
        public ColorMode SupportedColorModes => ColorMode.Format1bpp;

        /// <summary>
        /// The display width in pixels
        /// </summary>
        public int Width => imageBuffer.Width;

        /// <summary>
        /// The display height in pixels
        /// </summary>
        public int Height => imageBuffer.Height;

        /// <summary>
        /// The buffer the holds the pixel data for the display
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(8000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => spiComms.BusSpeed;
            set => spiComms.BusSpeed = value;
        }

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => spiComms.BusMode;
            set => spiComms.BusMode = value;
        }

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly ISpiCommunications spiComms;

        readonly IDigitalOutputPort dataCommandPort;
        readonly IDigitalOutputPort resetPort;

        const bool Data = true;
        const bool Command = false;

        readonly Buffer1bpp imageBuffer;
        readonly byte[] pageBuffer;

        /// <summary>
        /// Create a new Uc1609c object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Uc1609c(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 192, int height = 64) :
            this(spiBus, chipSelectPin?.CreateDigitalOutputPort(), dcPin.CreateDigitalOutputPort(),
                resetPin.CreateDigitalOutputPort(), width, height)
        {
        }

        /// <summary>
        /// Create a new Uc1609c display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Uc1609c(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            int width = 128, int height = 64)
        {
            this.dataCommandPort = dataCommandPort;
            this.resetPort = resetPort;

            spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

            imageBuffer = new Buffer1bpp(width, height);
            pageBuffer = new byte[192];

            Initialize();
        }

        private void Initialize()
        {
            Thread.Sleep(50);

            dataCommandPort.State = true;

            Reset();

            SendCommand(UC1609_TEMP_COMP_REG, UC1609_TEMP_COMP_SET);
            SendCommand(UC1609_ADDRESS_CONTROL, UC1609_ADDRESS_SET);
            SendCommand(UC1609_FRAMERATE_REG, UC1609_FRAMERATE_SET);
            SendCommand(UC1609_BIAS_RATIO, UC1609_BIAS_RATIO_SET);
            SendCommand(UC1609_POWER_CONTROL, UC1609_PC_SET);

            Thread.Sleep(100);

            SendCommand(UC1609_GN_PM, 0);
            SendCommand(UC1609_GN_PM, 0x1E); //  changed by user default = 0x49 //Constrast 00 to FE

            SendCommand(UC1609_DISPLAY_ON, 0x01); // turn on display
            SendCommand(UC1609_LCD_CONTROL, UC1609_ROTATION_NORMAL); // rotate to normal 
        }

        void SendCommand(byte command, byte value)
        {
            dataCommandPort.State = Command;
            SendData((byte)(command | value));
            dataCommandPort.State = Data;
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="data">Command byte to send to the display</param>
        private void SendData(byte data)
        {
            spiComms.Write(data);
        }

        private void Reset()
        {
            resetPort.State = false;
            Thread.Sleep(150);
            resetPort.State = true;
            Thread.Sleep(150);
        }

        /// <summary>
        /// Clear the display buffer
        /// </summary>
        /// <param name="updateDisplay">Immediately update the display when true</param>
        public void Clear(bool updateDisplay = false)
        {
            imageBuffer.Clear();

            if (updateDisplay) { Show(); }
        }

        /// <summary>
        /// Draw pixel at a location
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset</param>
        /// <param name="y">Ordinate of the pixel to the set / reset</param>
        /// <param name="color">Any color = turn on pixel, black = turn off pixel</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        /// Draw pixel at a location
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset</param>
        /// <param name="y">Ordinate of the pixel to the set / reset</param>
        /// <param name="enabled">True = turn on pixel, false = turn off pixel</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            imageBuffer.SetPixel(x, y, enabled);
        }

        /// <summary>
        /// Invert a pixel at a location
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset</param>
        /// <param name="y">Ordinate of the pixel to the set / reset</param>
        public void InvertPixel(int x, int y)
        {
            imageBuffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Fill display buffer with a color
        /// </summary>
        /// <param name="clearColor">The fill color</param>
        /// <param name="updateDisplay">If true, update display</param>
        public void Fill(Color clearColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(clearColor.Color1bpp);

            if (updateDisplay) Show();
        }

        /// <summary>
        /// Fill with a color
        /// </summary>
        /// <param name="x">X start position in pixels</param>
        /// <param name="y">Y start position in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="color">The fill color</param>
        public void Fill(int x, int y, int width, int height, Color color)
        {
            imageBuffer.Fill(x, y, width, height, color);
        }

        /// <summary>
        /// Write a buffer to the display offscreen buffer
        /// </summary>
        /// <param name="x">The x position in pixels to write the buffer</param>
        /// <param name="y">The y position in pixels to write the buffer</param>
        /// <param name="displayBuffer">The buffer to write</param>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Send the internal pixel buffer to display
        /// </summary>
        public void Show()
        {
            spiComms.Write(imageBuffer.Buffer);
        }

        public void Show(int left, int top, int right, int bottom)
        {
            throw new System.NotImplementedException();

            /*   for (int page = 0; page < 8; page++)
               {
                   SendCommand(UC1609_SET_COLADD_LSB, 0x0F);
                   SendCommand(UC1609_SET_COLADD_MSB, 0xF0 >> 4);
                   SendCommand(UC1609_SET_PAGEADD, (byte)page);

                   dataCommandPort.State = Data;

                   for (int i = 0; i < Width; i++)
                   {
                       spiComms.Write(imageBuffer.Buffer[Width * page + i]);
                   }

                   // Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 0, Width);
                   // spiComms.Write(pageBuffer);
               } */
        }
    }
}