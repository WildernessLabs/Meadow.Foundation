using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface to the Sh1106 family of displays
    /// </summary>
    public partial class Sh1106 : IGraphicsDisplay
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
        public int Width => 128;

        /// <summary>
        /// The display height in pixels
        /// </summary>
        public int Height => 64;

        /// <summary>
        /// The buffer the holds the pixel data for the display
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// SPI peripheral object
        /// </summary>
        ISpiPeripheral spiPerihperal;

        IDigitalOutputPort dataCommandPort;
        IDigitalOutputPort resetPort;

        const bool Data = true;
        const bool Command = false;

        Buffer1bpp imageBuffer;
        byte[] pageBuffer;

        /// <summary>
        /// Create a new Sh1106 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        public Sh1106(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin) :
            this(spiBus, chipSelectPin?.CreateDigitalOutputPort(), dcPin.CreateDigitalOutputPort(),
                resetPin.CreateDigitalOutputPort())
        {
        }

        /// <summary>
        /// Create a new Sh1106 display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        public Sh1106(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort)
        {
            this.dataCommandPort = dataCommandPort;
            this.resetPort = resetPort;

            spiPerihperal = new SpiPeripheral(spiBus, chipSelectPort);

            imageBuffer = new Buffer1bpp(Width, Height);
            pageBuffer = new byte[PageSize];

            Initialize();
        }

        /// <summary>
        /// Invert the entire display (true) or return to normal mode (false)
        /// </summary>
        public void InvertDisplay(bool invert)
        {
            if (invert)
            {
                SendCommand(DisplayCommand.DisplayVideoReverse);
            }
            else
            {
                SendCommand(DisplayCommand.DisplayVideoNormal);
            }
        }

        /// <summary>
        /// Set display into power saving mode
        /// </summary>
        public void PowerSaveMode()
        {
            SendCommand(DisplayCommand.DisplayOff);
            SendCommand(DisplayCommand.AllPixelsOn);
        }

        void SendCommand(DisplayCommand command)
        {
            SendCommand((byte)command);
        }

        void Reset()
        {
            resetPort.State = true;
            Thread.Sleep(10);
            resetPort.State = false;
            Thread.Sleep(10);
            resetPort.State = true;
            Thread.Sleep(100);
        }

        private void Initialize()
        {
            Reset();

            SendCommand(DisplayCommand.DisplayOff);
            SendCommand(DisplayCommand.SetDisplayClockDiv);
            //128x64 init commands
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
            SendCommand(0xA4);

            SendCommand(DisplayCommand.SetDisplayClockDiv);
            SendCommand(0xF0);

            SendCommand(DisplayCommand.DisplayOn);
        }

        /// <summary>
        /// Set the display contrast 
        /// </summary>
        /// <param name="contrast">The contrast value (0-63)</param>
        public void SetContrast(byte contrast)
        {
            SendCommand(DisplayCommand.SetContrast);
            SendCommand(contrast);
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">Command byte to send to the display</param>
        private void SendCommand(byte command)
        {
            dataCommandPort.State = Command;
            spiPerihperal.Write(command);
        }

        /// <summary>
        /// Send a sequence of commands to the display
        /// </summary>
        /// <param name="commands">List of commands to send</param>
        private void SendCommands(byte[] commands)
        {
            var data = new byte[commands.Length + 1];
            data[0] = 0x00;
            Array.Copy(commands, 0, data, 1, commands.Length);

            dataCommandPort.State = Command;
            spiPerihperal.Write(commands);
        }

        const int StartColumnOffset = 0;
        const int PageSize = 128;

        /// <summary>
        /// Send the internal pixel buffer to display
        /// </summary>
        public void Show()
        {
            for (int page = 0; page < 8; page++)
            {
                SendCommand(DisplayCommand.ColumnAddressLow);
                SendCommand(DisplayCommand.ColumnAddressHigh);
                SendCommand((byte)((byte)DisplayCommand.PageAddress | page));

                dataCommandPort.State = Data;

                Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 0, PageSize);
                spiPerihperal.Write(pageBuffer);

                dataCommandPort.State = Command;
            }
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
            const int pageHeight = 8;

            //must update in pages (area of 128x8 pixels)
            //so interate over all 8 pages and check if they're in range
            for (int page = 0; page < 8; page++)
            {
                if (top > pageHeight * page || bottom < (page + 1) * pageHeight)
                {
                    continue;
                }

                SendCommand((byte)((int)DisplayCommand.PageAddress | page));
                SendCommand((DisplayCommand.ColumnAddressLow) | (StartColumnOffset & 0x0F));
                SendCommand((int)DisplayCommand.ColumnAddressHigh | 0);

                dataCommandPort.State = Data;

                Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 0, PageSize);
                spiPerihperal.Write(pageBuffer);
            }
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
        /// Start the display scrollling in the specified direction.
        /// </summary>
        /// <param name="direction">Direction that the display should scroll</param>
        public void StartScrolling(ScrollDirection direction)
        {
            StartScrolling(direction, 0x00, 0xff);
        }

        /// <summary>
        /// Start the display scrolling
        /// </summary>
        /// <remarks>
        /// In most cases setting startPage to 0x00 and endPage to 0xff will achieve an
        /// acceptable scrolling effect
        /// </remarks>
        /// <param name="direction">Direction that the display should scroll</param>
        /// <param name="startPage">Start page for the scroll</param>
        /// <param name="endPage">End oage for the scroll</param>
        public void StartScrolling(ScrollDirection direction, byte startPage, byte endPage)
        {
            StopScrolling();
            byte[] commands;
            if ((direction == ScrollDirection.Left) || (direction == ScrollDirection.Right))
            {
                commands = new byte[] { 0x26, 0x00, startPage, 0x00, endPage, 0x00, 0xff, 0x2f };

                if (direction == ScrollDirection.Left)
                {
                    commands[0] = 0x27;
                }
            }
            else
            {
                byte scrollDirection;

                if (direction == ScrollDirection.LeftAndVertical)
                {
                    scrollDirection = 0x2a;
                }
                else
                {
                    scrollDirection = 0x29;
                }

                commands = new byte[] { 0xa3, 0x00, (byte)Height, scrollDirection, 0x00, startPage, 0x00, endPage, 0x01, 0x2f };
            }
            SendCommands(commands);
        }

        /// <summary>
        /// Turn off scrolling
        /// </summary>
        /// <remarks>
        /// Datasheet states that scrolling must be turned off before changing the
        /// scroll direction in order to prevent RAM corruption
        /// </remarks>
        public void StopScrolling()
        {
            SendCommand(0x2e);
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
    }
}