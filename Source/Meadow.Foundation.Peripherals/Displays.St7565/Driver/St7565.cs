using System;
using System.Threading;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    ///     Provide an interface to the ST7565 family of displays.
    /// </summary>
    public partial class St7565 : IGraphicsDisplay
    {
        public ColorType ColorMode => ColorType.Format1bpp;

        public int Width => imageBuffer.Width;

        public int Height => imageBuffer.Height;

        public bool IgnoreOutOfBoundsPixels { get; set; }

        /// <summary>
        ///     SPI object
        /// </summary>
        protected ISpiPeripheral spiPerihperal;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;

        protected const bool Data = true;
        protected const bool Command = false;

        protected Buffer1bpp imageBuffer;
        protected byte[] pageBuffer;

        /// <summary>
        ///     Invert the entire display (true) or return to normal mode (false).
        /// </summary>
        public void InvertDisplay(bool cmd)
        {
            if (cmd)
            {
                SendCommand(DisplayCommand.DisplayVideoReverse);
            }
            else
            {
                SendCommand(DisplayCommand.DisplayVideoNormal);
            }
        }

        public void PowerSaveMode()
        {
            SendCommand(DisplayCommand.DisplayOff);
            SendCommand(DisplayCommand.AllPixelsOn);
        }

        /// <summary>
        ///     Create a new ST7565 object using the default parameters for
        /// </summary>
        public St7565(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 128, int height = 64)
        {
            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            resetPort = device.CreateDigitalOutputPort(resetPin, false);
            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin);

            spiPerihperal = new SpiPeripheral(spiBus, chipSelectPort);

            imageBuffer = new Buffer1bpp(width, height);
            pageBuffer = new byte[PageSize];

            InitST7565();
        }

        void SendCommand(DisplayCommand command)
        {
            SendCommand((byte)command);
        }

        private void InitST7565()
        {
            resetPort.State = false;
            Thread.Sleep(50);
            resetPort.State = true;

            SendCommand(DisplayCommand.LcdVoltageBias7);
            SendCommand(DisplayCommand.AdcSelectNormal);
            SendCommand(DisplayCommand.ShlSelectReverse);
            SendCommand((int)(DisplayCommand.DisplayStartLine) | 0x00);

            SendCommand(((int)(DisplayCommand.PowerControl) | 0x04)); // turn on voltage converter (VC=1, VR=0, VF=0)
            Thread.Sleep(50);
            SendCommand(((int)(DisplayCommand.PowerControl) | 0x06)); // turn on voltage regulator (VC=1, VR=1, VF=0)
            Thread.Sleep(50);
            SendCommand(((int)(DisplayCommand.PowerControl) | 0x07)); // turn on voltage follower (VC=1, VR=1, VF=1)
            Thread.Sleep(50);

            SendCommand(((int)(DisplayCommand.RegulatorResistorRatio) | 0x06)); // set lcd operating voltage (regulator resistor, ref voltage resistor)

            SendCommand(DisplayCommand.DisplayOn);
            SendCommand(DisplayCommand.AllPixelsOff);
        }

        public const uint ContrastHigh = 34;
        public const uint ContrastMedium = 24;
        public const uint ContrastLow = 15;

        // 0-63
        public void SetContrast(uint contrast)
        {
            SendCommand(DisplayCommand.ContrastRegister);
            SendCommand((byte)((int)(DisplayCommand.ContrastValue) | (contrast & 0x3f)));
        }

        /// <summary>
        ///     Send a command to the display.
        /// </summary>
        /// <param name="command">Command byte to send to the display.</param>
        private void SendCommand(byte command)
        {
            dataCommandPort.State = Command;
            spiPerihperal.Write(command);
        }

        /// <summary>
        ///     Send a sequence of commands to the display.
        /// </summary>
        /// <param name="commands">List of commands to send.</param>
        private void SendCommands(byte[] commands)
        {
            var data = new byte[commands.Length + 1];
            data[0] = 0x00;
            Array.Copy(commands, 0, data, 1, commands.Length);

            dataCommandPort.State = Command;
            spiPerihperal.Write(commands);
        }

        protected const int StartColumnOffset = 0; // 1;
        protected const int PageSize = 128;

        /// <summary>
        ///     Send the internal pixel buffer to display.
        /// </summary>
        public void Show()
        {
            for (int page = 0; page < 8; page++)
            {
                SendCommand((byte)((int)DisplayCommand.PageAddress | page));
                SendCommand((DisplayCommand.ColumnAddressLow) | (StartColumnOffset & 0x0F));
                SendCommand((int)DisplayCommand.ColumnAddressHigh | 0);
                SendCommand(DisplayCommand.EnterReadModifyWriteMode);

                dataCommandPort.State = Data;

                Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 0, PageSize);
                spiPerihperal.Write(pageBuffer);
            }
        }

        public void Show(int left, int top, int right, int bottom)
        {
            const int pageHeight = 8;

            //must update in pages (area of 128x8 pixels)
            //so interate over all 8 pages and check if they're in range
            for (int page = 0; page < 8; page++)
            {
                if(top > pageHeight*page || bottom < (page + 1) * pageHeight)
                {
                    continue;
                }

                SendCommand((byte)((int)(DisplayCommand.PageAddress) | page));
                SendCommand((DisplayCommand.ColumnAddressLow) | (StartColumnOffset & 0x0F));
                SendCommand((int)DisplayCommand.ColumnAddressHigh | 0);
                SendCommand(DisplayCommand.EnterReadModifyWriteMode);

                dataCommandPort.State = Data;

                Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 0, PageSize);
                spiPerihperal.Write(pageBuffer);
            }
        }

        /// <summary>
        ///     Clear the display buffer.
        /// </summary>
        /// <param name="updateDisplay">Immediately update the display when true.</param>
        public void Clear(bool updateDisplay = false)
        {
            imageBuffer.Clear();

            if (updateDisplay) { Show(); }
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="color">Any color = turn on pixel, black = turn off pixel</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="colored">True = turn on pixel, false = turn off pixel</param>
        public void DrawPixel(int x, int y, bool colored)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            imageBuffer.SetPixel(x, y, colored);
        }

        public void InvertPixel(int x, int y)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            var index = (y / 8 * Width) + x;

            imageBuffer.Buffer[index] = (imageBuffer.Buffer[index] ^= (byte)(1 << y % 8));
        }

        /// <summary>
        ///     Start the display scrollling in the specified direction.
        /// </summary>
        /// <param name="direction">Direction that the display should scroll.</param>
        public void StartScrolling(ScrollDirection direction)
        {
            StartScrolling(direction, 0x00, 0xff);
        }

        /// <summary>
        ///     Start the display scrolling.
        /// </summary>
        /// <remarks>
        ///     In most cases setting startPage to 0x00 and endPage to 0xff will achieve an
        ///     acceptable scrolling effect.
        /// </remarks>
        /// <param name="direction">Direction that the display should scroll.</param>
        /// <param name="startPage">Start page for the scroll.</param>
        /// <param name="endPage">End oage for the scroll.</param>
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

                commands = new byte[] { 0xa3, 0x00, (byte) Height, scrollDirection, 0x00, startPage, 0x00, endPage, 0x01, 0x2f };
            }
            SendCommands(commands);
        }

        /// <summary>
        ///     Turn off scrolling.
        /// </summary>
        /// <remarks>
        ///     Datasheet states that scrolling must be turned off before changing the
        ///     scroll direction in order to prevent RAM corruption.
        /// </remarks>
        public void StopScrolling()
        {
            SendCommand(0x2e);
        }

        public void Fill(Color clearColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(clearColor.Color1bpp);

            if (updateDisplay) Show();
        }

        public void Fill(int x, int y, int width, int height, Color color)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x > width - 1) x = width - 1;
                if (y > height - 1) y = height - 1;
            }

            imageBuffer.Fill(color, x, y, width, height);
        }

        public void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }
    }
}