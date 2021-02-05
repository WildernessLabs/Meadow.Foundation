using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface to the SSD1306 family of OLED displays.
    /// </summary>
    public class Ssd1306 : DisplayBase
    {
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

        /// <summary>
        ///     Supported display types.
        /// </summary>
        public enum DisplayType
        {
            /// <summary>
            ///     0.96 128x64 pixel display.
            /// </summary>
            OLED128x64,
            /// <summary>
            ///     0.91 128x32 pixel display.
            /// </summary>
            OLED128x32,
            /// <summary>
            ///     64x48 pixel display.
            /// </summary>
            OLED64x48,
            /// <summary>
            ///     96x16 pixel display.
            /// </summary>
            OLED96x16,
            /// <summary>
            ///     70x40 pixel display.
            /// </summary>
            OLED72x40,
        }

        public enum ConnectionType
        {
            SPI,
            I2C,
        }

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override int Width => width;

        public override int Height => height;

        /// <summary>
        ///     SSD1306 SPI display
        /// </summary>
        protected ISpiPeripheral spiPeripheral;
        protected SpiBus spi;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ConnectionType connectionType;
        protected const bool Data = true;
        protected const bool Command = false;

        /// <summary>
        ///     SSD1306 I2C display
        /// </summary>
        private readonly II2cPeripheral i2cPeripheral;

        /// <summary>
        ///     Width of the display in pixels.
        /// </summary>
        private int width;

        /// <summary>
        ///     Height of the display in pixels.
        /// </summary>
        private int height;

        /// <summary>
        ///     X offset for non-standard displays.
        /// </summary>
        private int xOffset = 0;

        /// <summary>
        ///     X offset for non-standard displays.
        /// </summary>
        private int yOffset = 0;

        private Color currentPen;

        /// <summary>
        ///     Buffer holding the pixels in the display.
        /// </summary>
        private byte[] buffer;
        private byte[] spiReceive;

        /// <summary>
        ///     Sequence of command bytes that must be sent to the display before
        ///     the Show method can send the data buffer.
        /// </summary>
        private byte[] showPreamble;

        /// <summary>
        ///     Sequence of bytes that should be sent to a 128x64 OLED display to setup the device.
        /// </summary>
        private readonly byte[] oled128x64SetupSequence =
        {
            0xae, 0xd5, 0x80, 0xa8, 0x3f, 0xd3, 0x00, 0x40 | 0x0, 0x8d, 0x14, 0x20, 0x00, 0xa0 | 0x1, 0xc8,
            0xda, 0x12, 0x81, 0xcf, 0xd9, 0xf1, 0xdb, 0x40, 0xa4, 0xa6, 0xaf
        };

        /// <summary>
        ///     Sequence of bytes that should be sent to a 128x32 OLED display to setup the device.
        /// </summary>
        private readonly byte[] oled128x32SetupSequence =
        {
            0xae, 0xd5, 0x80, 0xa8, 0x1f, 0xd3, 0x00, 0x40 | 0x0, 0x8d, 0x14, 0x20, 0x00, 0xa0 | 0x1, 0xc8,
            0xda, 0x02, 0x81, 0x8f, 0xd9, 0x1f, 0xdb, 0x40, 0xa4, 0xa6, 0xaf
        };

        /// <summary>
        ///     Sequence of bytes that should be sent to a 72x40 OLED display to setup the device.
        /// </summary>
        private readonly byte[] oled72x40SetupSequence =
        {
            0xae, 0xd5, 0x80, 0xa8, 0x27, 0xd3, 0x00, 0xad, 0x30, 0x40, 0x8d, 0x14, 0x20, 0x00, 0xa1, 0xc8,
            0xda, 0x12, 0x81, 0xcf, 0xd9, 0xf1, 0xdb, 0x40, 0xa4, 0xa6, 0xaf
        };

        /// <summary>
        ///     Sequence of bytes that should be sent to a 96x16 OLED display to setup the device.
        /// </summary>
        private readonly byte[] oled96x16SetupSequence =
        {
            0xae, 0xd5, 0x80, 0xa8, 0x1f, 0xd3, 0x00, 0x40 | 0x0, 0x8d, 0x14, 0x20, 0x00, 0xa0 | 0x1, 0xc8,
            0xda, 0x02, 0x81, 0xaf, 0xd9, 0x1f, 0xdb, 0x40, 0xa4, 0xa6, 0xaf
        };
        /// <summary>
        ///     Sequence of bytes that should be sent to a 64x48 OLED display to setup the device.
        /// </summary>
        private readonly byte[] oled64x48SetupSequence =
        {
            0xae, 0xd5, 0x80, 0xa8, 0x3f, 0xd3, 0x00, 0x40 | 0x0, 0x8d, 0x14, 0x20, 0x00, 0xa0 | 0x1, 0xc8,
            0xda, 0x12, 0x81, 0xcf, 0xd9, 0xf1, 0xdb, 0x40, 0xa4, 0xa6, 0xaf
        };

        /// <summary>
        ///     Backing variable for the InvertDisplay property.
        /// </summary>
        private bool invertDisplay;

        /// <summary>
        ///     Invert the entire display (true) or return to normal mode (false).
        /// </summary>
        /// <remarks>
        ///     See section 10.1.10 in the datasheet.
        /// </remarks>
        public bool InvertDisplay
        {
            get => invertDisplay; 
            set
            {
                invertDisplay = value;
                SendCommand((byte)(value ? 0xa7 : 0xa6));
            }
        }

        /// <summary>
        ///     Backing variable for the Contrast property.
        /// </summary>
        private byte contrast;

        /// <summary>
        ///     Get / Set the contrast of the display.
        /// </summary>
        public byte Contrast
        {
            get => contrast; 

            set
            {
                contrast = value;
                SendCommands(new byte[] { 0x81, contrast });
            }
        }

        /// <summary>
        ///     Put the display to sleep (turns the display off).
        /// </summary>
        public bool Sleep
        {
            get => sleep;
            set
            {
                sleep = value;
                SendCommand((byte)(sleep ? 0xae : 0xaf));
            }
        }

        /// <summary>
        ///     Backing variable for the Sleep property.
        /// </summary>
        private bool sleep;

        private DisplayType displayType;

        /// <summary>
        ///     Create a new SSD1306 object using the default parameters for
        /// </summary>
        /// <remarks>
        ///     Note that by default, any pixels out of bounds will throw and exception.
        ///     This can be changed by setting the <seealso cref="IgnoreOutOfBoundsPixels" />
        ///     property to true.
        /// </remarks>
        /// <param name="displayType">Type of SSD1306 display (default = 128x64 pixel display).</param>
        ///
        public Ssd1306(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            DisplayType displayType = DisplayType.OLED128x64)
        {
            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            resetPort = device.CreateDigitalOutputPort(resetPin, true);
            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin, false);

            spi = (SpiBus)spiBus;
            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            connectionType = ConnectionType.SPI;

            InitSSD1306(displayType); 
        }

        /// <summary>
        ///     Create a new SSD1306 object using the default parameters for
        /// </summary>
        /// <remarks>
        ///     Note that by default, any pixels out of bounds will throw and exception.
        ///     This can be changed by setting the <seealso cref="IgnoreOutOfBoundsPixels" />
        ///     property to true.
        /// </remarks>
        /// <param name="address">Address of the bus on the I2C display.</param>
        /// <param name="displayType">Type of SSD1306 display (default = 128x64 pixel display).</param>
        public Ssd1306(II2cBus i2cBus, 
            byte address = 0x3c, DisplayType displayType = DisplayType.OLED128x64)
        {
            this.displayType = displayType;

            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            connectionType = ConnectionType.I2C;

            InitSSD1306(displayType);
        }

        private void InitSSD1306 (DisplayType displayType)
        { 
            switch (displayType)
            {
                case DisplayType.OLED128x64:
                    width = 128;
                    height = 64;
                    SendCommands(oled128x64SetupSequence);
                    break;
                case DisplayType.OLED64x48:
                    width = 128;
                    height = 64;
                    xOffset = 32;
                    yOffset = 16;
                    SendCommands(oled128x64SetupSequence);
                    break;
                case DisplayType.OLED72x40:
                    width = 72;
                    height = 40;
                    SendCommands(oled72x40SetupSequence);
                    break;
                case DisplayType.OLED128x32:
                    width = 128;
                    height = 32;
                    SendCommands(oled128x32SetupSequence);
                    break;
                case DisplayType.OLED96x16:
                    width  = 96;
                    height = 16;
                    SendCommands(oled96x16SetupSequence);
                    break;
            }

            //align buffer to PAGE_SIZE
            int bufferSize = width * height / 8;
            bufferSize += bufferSize % 16;

            buffer = new byte[bufferSize];

            if(connectionType == ConnectionType.SPI)
            {
                spiReceive = new byte[width * height / 8];
            }

            showPreamble = new byte[] { 0x21, 0x00, (byte)(width - 1), 0x22, 0x00, (byte)(height/8 - 1) };

            IgnoreOutOfBoundsPixels = false;

            //
            //  Finally, put the display into a known state.
            //
            InvertDisplay = false;
            Sleep = false;
            Contrast = 0xff;
            StopScrolling();
        }

        /// <summary>
        ///     Send a command to the display.
        /// </summary>
        /// <param name="command">Command byte to send to the display.</param>
        private void SendCommand(byte command)
        {
            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort.State = Command;
                spiPeripheral.WriteByte(command);
            }
            else
            {
                i2cPeripheral.WriteBytes(new byte[] { 0x00, command });
            }
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

            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort.State = Command;
                spiPeripheral.WriteBytes(commands);
            }
            else
            {
                i2cPeripheral.WriteBytes(data);
            }
        }

        /// <summary>
        ///     Send the internal pixel buffer to display.
        /// </summary>
        public override void Show()
        {
            SendCommands(showPreamble);
            //
            //  Send the buffer page by page.
            //
            const int PAGE_SIZE = 16;
            var data = new byte[PAGE_SIZE + 1];
            data[0] = 0x40;

            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort.State = Data;
                spi.ExchangeData(chipSelectPort, ChipSelectMode.ActiveLow, buffer, spiReceive);
            }
            else
            {
                for (ushort index = 0; index < buffer.Length; index += PAGE_SIZE)
                {
                    if(buffer.Length - index < PAGE_SIZE) { break; }

                    Array.Copy(buffer, index, data, 1, PAGE_SIZE);
                    SendCommand(0x40);
                    i2cPeripheral.WriteBytes(data);
                }
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

        /// <summary>
        ///     Set the pen color, black is off, any other color is on
        /// </summary>   
        public override void SetPenColor(Color pen)
        {
            currentPen = pen;
        }

        /// <summary>
        ///     Draw a pixel to the display using the pen
        /// </summary>    
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, currentPen);
        }

        /// <summary>
        ///     Draw a pixel to the display - coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="color">Black - pixel off, any color - turn on pixel</param>
        public override void DrawPixel(int x, int y, Color color)
        {
            var colored = (color == Color.Black) ? false : true;

            DrawPixel(x, y, colored);
        }

        /// <summary>
        ///     Draw a pixel to the display - coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="colored">True = turn on pixel, false = turn off pixel</param>
        public override void DrawPixel(int x, int y, bool colored)
        {
            /*  if(_displayType == DisplayType.OLED64x48)
              {
                  DrawPixel64x48(x, y, colored);
                  return;
              } */

            x += xOffset;
            y += yOffset;

            if (IgnoreOutOfBoundsPixels)
            {
                if ((x >= width) || (y >= height) || x < 0 || y < 0)
                {
                    //  pixels to be thrown away if out of bounds of the display
                    return;
                }
            }
            
            var index = (y >> 3) * width + x; //divide by 8

            if (colored)
            {
                buffer[index] = (byte) (buffer[index] | (byte) (1 << (y % 8)));
            }
            else
            {
                buffer[index] = (byte) (buffer[index] & ~(byte) (1 << (y % 8)));
            }
        }

        public override void InvertPixel(int x, int y)
        {
            x += xOffset;
            y += yOffset;
        
            if (IgnoreOutOfBoundsPixels)
            {
                if ((x >= width) || (y >= height))
                {
                    return;
                }
            }
            var index = (y >> 8) * width + x;

            buffer[index] = (buffer[index] ^= (byte)(1 << y % 8));
        }

        private void DrawPixel64x48(int x, int y, bool colored)
        {
            if ((x >= 64) || (y >= 48))
            {
                if (!IgnoreOutOfBoundsPixels)
                {
                    throw new ArgumentException("DisplayPixel: co-ordinates out of bounds");
                }
                //  pixels to be thrown away if out of bounds of the display
                return;
            }

            //offsets for landscape
            x += 32;
            y += 16;

            var index = (y / 8 * width) + x;

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
                commands = new byte[]
                    { 0xa3, 0x00, (byte) height, scrollDirection, 0x00, startPage, 0x00, endPage, 0x01, 0x2f };
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
    }
}