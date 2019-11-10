using System;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    ///     Provide an interface to the ST7565 family of displays.
    /// </summary>
    public class ST7565 : DisplayBase
    {
        #region Enums

        /// <summary>
        ///     Allow the programmer to set the scroll direction.
        /// </summary>
        public enum ScrollDirection
        {
            /// <summary>
            ///     Scroll the display to the left.
            /// </summary>
            Left,
            /// <summary>
            ///     Scroll the display to the right.
            /// </summary>
            Right,
            /// <summary>
            ///     Scroll the display from the bottom left and vertically.
            /// </summary>
            RightAndVertical,
            /// <summary>
            ///     Scroll the display from the bottom right and vertically.
            /// </summary>
            LeftAndVertical
        }

        enum DisplayCommand : byte
        {
            DisplayOff = 0xAE,
            DisplayOn = 0xAF,
            DisplayStartLine = 0x40,
            PageAddress = 0xB0,
            ColumnAddressHigh = 0x10,
            ColumnAddressLow = 0x00,
            AdcSelectNormal = 0xA0, // X axis flip OFF
            AdcSelectReverse = 0xA1, // X axis flip ON
            DisplayVideoNormal = 0xA6,
            DisplayVideoReverse = 0xA7,
            AllPixelsOff = 0xA4,
            AllPixelsOn = 0xA5,
            LcdVoltageBias9 = 0xA2,
            LcdVoltageBias7 = 0xA3,
            EnterReadModifyWriteMode = 0xE0,
            ClearReadModifyWriteMode = 0xEE,
            ResetLcdModule = 0xE2,
            ShlSelectNormal = 0xC0, // Y axis flip OFF
            ShlSelectReverse = 0xC8, // Y axis flip ON
            PowerControl = 0x28,
            RegulatorResistorRatio = 0x20,
            ContrastRegister = 0x81,
            ContrastValue = 0x00,
            NoOperation = 0xE3
        }

        #endregion Enums

        #region Member variables / fields

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override uint Width { get; }

        public override uint Height { get; }

        /// <summary>
        ///     SPI object
        /// </summary>
        protected ISpiPeripheral spiPerihperal;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;

        protected const bool Data = true;
        protected const bool Command = false;

        protected Color currentPen = Color.White;

        /// <summary>
        ///     Buffer holding the pixels in the display.
        /// </summary>
        private byte[] buffer;

        #endregion Member variables / fields

        #region Properties

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

        #endregion Properties

        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent it being used.
        /// </summary>
        private ST7565() { }

        /// <summary>
        ///     Create a new ST7565 object using the default parameters for
        /// </summary>
        public ST7565(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            uint width = 128, uint height = 64)
        {
            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            resetPort = device.CreateDigitalOutputPort(resetPin, false);
            var chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin);

            spiPerihperal = new SpiPeripheral(spiBus, chipSelectPort);

            Width = width;
            Height = height;

            InitST7565();
        }

        void SendCommand(DisplayCommand command)
        {
            SendCommand((byte)command);
        }

        private void InitST7565()
        {
            buffer = new byte[Width * Height / 8];

            IgnoreOutOfBoundsPixels = false;

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

        #endregion Constructors

        #region Methods

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
            spiPerihperal.WriteBytes(new byte[] { command });
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
            spiPerihperal.WriteBytes(commands);
        }

        protected const int StartColumnOffset = 0; // 1;
        protected const int pageSize = 128;
        protected int[] pageReference = { 4, 5, 6, 7, 0, 1, 2, 3 };
        protected byte[] pageBuffer = new byte[pageSize];

        /// <summary>
        ///     Send the internal pixel buffer to display.
        /// </summary>
        public override void Show()
        {
            for (int page = 0; page < 8; page++)
            {
                SendCommand((byte)((int)(DisplayCommand.PageAddress) | page));
                SendCommand((DisplayCommand.ColumnAddressLow) | (StartColumnOffset & 0x0F));
                SendCommand((int)(DisplayCommand.ColumnAddressHigh) | 0);
                SendCommand(DisplayCommand.EnterReadModifyWriteMode);

                dataCommandPort.State = Data;
                Array.Copy(buffer, (int)(Width * page), pageBuffer, 0, pageSize);
                spiPerihperal.WriteBytes(pageBuffer);
            }
        }

        /// <summary>
        ///     Clear the display buffer.
        /// </summary>
        /// <param name="updateDisplay">Immediately update the display when true.</param>
        public override void Clear(bool updateDisplay = false)
        {
            Array.Clear(buffer, 0, buffer.Length);

            if (updateDisplay)
            {
                Show();
            }
        }

        public override void SetPenColor(Color pen)
        {
            currentPen = pen;
        }

        /// <summary>
        ///     Draw pixel using current pen
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, currentPen);
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="color">Any color = turn on pixel, black = turn off pixel</param>
        public override void DrawPixel(int x, int y, Color color)
        {
            var colored = (color == Color.Black) ? false : true;

            DrawPixel(x, y, colored);
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="colored">True = turn on pixel, false = turn off pixel</param>
        public override void DrawPixel(int x, int y, bool colored)
        {
            if ((x >= Width) || (y >= Height))
            {
                if (!IgnoreOutOfBoundsPixels)
                {
                    throw new ArgumentException("DisplayPixel: co-ordinates out of bounds");
                }
                //  pixels to be thrown away if out of bounds of the display
                return;
            }
            var index = (y / 8 * Width) + x;

            if (colored)
            {
                buffer[index] = (byte)(buffer[index] | (byte)(1 << (y % 8)));
            }
            else
            {
                buffer[index] = (byte)(buffer[index] & ~(byte)(1 << (y % 8)));
            }
        }

        /// <summary>
        ///     Copy a bitmap to the display.
        /// </summary>
        /// <remarks>
        ///     Currently, this method only supports copying the bitmap over the contents
        ///     of the display buffer.
        /// </remarks>
        /// <param name="x">Abscissa of the top left corner of the bitmap.</param>
        /// <param name="y">Ordinate of the top left corner of the bitmap.</param>
        /// <param name="width">Width of the bitmap in bytes.</param>
        /// <param name="height">Height of the bitmap in bytes.</param>
        /// <param name="bitmap">Bitmap to transfer</param>
        /// <param name="bitmapMode">How should the bitmap be transferred to the display?</param>
        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, BitmapMode bitmapMode)
        {
            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }
            for (var ordinate = 0; ordinate < height; ordinate++)
            {
                for (var abscissa = 0; abscissa < width; abscissa++)
                {
                    var b = bitmap[(ordinate * width) + abscissa];
                    byte mask = 0x01;
                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        DrawPixel(x + (8 * abscissa) + pixel, y + ordinate, (b & mask) > 0);
                        mask <<= 1;
                    }
                }
            }
        }

        //needs dithering code
        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, Color color)
        {
            DrawBitmap(x, y, width, height, bitmap, BitmapMode.And);
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

        #endregion Methods
    }
}