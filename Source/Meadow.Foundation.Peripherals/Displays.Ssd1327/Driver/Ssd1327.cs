﻿using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provides an interface to the Ssd1327 greyscale OLED display
    /// </summary>
    public partial class Ssd1327 : IGraphicsDisplay
    {
        /// <summary>
        /// The display color mode (4 bit per pixel grayscale)
        /// </summary>
        public ColorType ColorMode => ColorType.Format4bppGray;

        /// <summary>
        /// The display width in pixels
        /// </summary>
        public int Width => 128;

        /// <summary>
        /// The display height in pixels
        /// </summary>
        public int Height => 128;

        /// <summary>
        /// The buffer the holds the pixel data for the display
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        readonly ISpiPeripheral spiPeripheral;
        readonly IDigitalOutputPort dataCommandPort;
        readonly IDigitalOutputPort resetPort;
        readonly IDigitalOutputPort chipSelectPort;

        readonly BufferGray4 imageBuffer;

        const bool DataState = true;
        const bool CommandState = false;

        /// <summary>
        /// Create a new Ssd1327 object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        public Ssd1327(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin)
        {
            imageBuffer = new BufferGray4(Width, Height);

            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            if (resetPin != null) { resetPort = device.CreateDigitalOutputPort(resetPin, true); }
            if (chipSelectPin != null) { chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin, false); }

            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            Initialize();
        }

        /// <summary>
        /// Create a new Ssd1327 display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        public Ssd1327(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort)
        {
            this.dataCommandPort = dataCommandPort;
            this.chipSelectPort = chipSelectPort;
            this.resetPort = resetPort;

            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            Initialize();
        }

        /// <summary>
        /// Set the disply contrast
        /// </summary>
        /// <param name="contrast">The constrast value (0-255)</param>
        public void SetContrast(byte contrast)
        {
            SendCommand(0x81);  //set contrast control
            SendCommand(contrast);  //Contrast byte
        }

        /// <summary>
        /// Initalize the display
        /// </summary>
        protected void Initialize()
        {
            Resolver.Log.Info("Initialize");

            dataCommandPort.State = CommandState;

            spiPeripheral.Write(init128x128);

            Thread.Sleep(100);              // 100ms delay recommended
            SendCommand(Command.DISPLAYON); // 0xaf
            SetContrast(0x2F);
        }

        /// <summary>
        /// Clear the display buffer
        /// </summary>
        /// <param name="updateDisplay">If true, update the display</param>
        public void Clear(bool updateDisplay = false)
        {
            Array.Clear(imageBuffer.Buffer, 0, imageBuffer.ByteCount);

            if(updateDisplay == true)
            {
                Show();
            }
        }

        /// <summary>
        /// Draw pixel at a location
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <param name="color">The pixel color which will be transformed to 4bpp greyscale</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color4bppGray);
        }

        /// <summary>
        /// Draw pixel at a location
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <param name="enabled">True = turn on pixel, false = turn off pixel</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            DrawPixel(x, y, (byte)(enabled ? 0x0F : 0));
        }

        /// <summary>
        /// Draw pixel at a location
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <param name="gray">The pixel color as a 4 bit grayscale value</param>
        public void DrawPixel(int x, int y, byte gray)
        {
            imageBuffer.SetPixel(x, y, gray);
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
        /// Update a region of the display from the offscreen buffer
        /// </summary>
        /// <param name="left">Left bounds in pixels</param>
        /// <param name="top">Top bounds in pixels</param>
        /// <param name="right">Right bounds in pixels</param>
        /// <param name="bottom">Bottom bounds in pixels</param>
        public void Show(int left, int top, int right, int bottom)
        {
            // SetAddressWindow(left, top, (byte)(right - left), (byte)(top - bottom));
            //ToDo this should be pretty easy to implement with Memory<byte>
            Show();
        }

        /// <summary>
        /// Update the display
        /// </summary>
        public void Show()
        {
            SetAddressWindow(0, 0, 127, 127);

            dataCommandPort.State = DataState;

            spiPeripheral.Write(imageBuffer.Buffer);
        }

        void SetAddressWindow(byte x0, byte y0, byte x1, byte y1)
        {
            SendCommand(Command.SETCOLUMN); //Set Column Address
            SendCommand(x0); //Beginning. Note that you must divide the column by 2, since 1 byte in memory is 2 pixels
            SendCommand((byte)(x1/2)); //End

            SendCommand(Command.SETROW); //Set Row Address
            SendCommand(y0); //Beginning
            SendCommand(y1); //End
        }

        void SendCommand(Command command)
        {
            SendCommand((byte)command);
        }

        void SendCommand(byte command)
        {
            dataCommandPort.State = CommandState;
            spiPeripheral.Write(command);
        }

        void SendData(int data)
        {
            SendData((byte)data);
        }

        void SendData(byte data)
        {
            dataCommandPort.State = DataState;
            spiPeripheral.Write(data);
        }

        void SendData(byte[] data)
        {
            dataCommandPort.State = DataState;
            spiPeripheral.Write(data);
        }

        /// <summary>
        /// Fill display buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        /// /// <param name="updateDisplay">If true, update display</param>
        public void Fill(Color color, bool updateDisplay = false)
        {
            imageBuffer.Fill(color);
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

        // Init sequence, make sure its under 32 bytes, or split into multiples
        readonly byte[] init128x128 = {
              // Init sequence for 128x32 OLED module
              (byte)Command.DISPLAYOFF, // 0xAE
              (byte)Command.SETCONTRAST,
              0x80,             // 0x81, 0x80
              (byte)Command.SEGREMAP, // 0xA0 0x53
              0x51, // remap memory, odd even columns, com flip and column swap
              (byte)Command.SETSTARTLINE,
              0x00, // 0xA1, 0x00
              (byte)Command.SETDISPLAYOFFSET,
              0x00, // 0xA2, 0x00
              (byte)Command.DISPLAYALLOFF, 
              (byte)Command.SETMULTIPLEX,
              0x7F, // 0xA8, 0x7F (1/64)
              (byte)Command.PHASELEN,
              0x11, // 0xB1, 0x11
              /*
              GRAYTABLE,
              0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
              0x07, 0x08, 0x10, 0x18, 0x20, 0x2f, 0x38, 0x3f,
              */
              (byte)Command.DCLK,
              0x00, // 0xb3, 0x00 (100hz)
              (byte)Command.REGULATOR,
              0x01, // 0xAB, 0x01
              (byte)Command.PRECHARGE2,
              0x04, // 0xB6, 0x04
              (byte)Command.SETVCOM,
              0x0F, // 0xBE, 0x0F
              (byte)Command.PRECHARGE,
              0x08, // 0xBC, 0x08
              (byte)Command.FUNCSELB,
              0x62, // 0xD5, 0x62
              (byte)Command.CMDLOCK,
              0x12, // 0xFD, 0x12
              (byte)Command.NORMALDISPLAY, 
              (byte)Command.DISPLAYON
        };
    }
}