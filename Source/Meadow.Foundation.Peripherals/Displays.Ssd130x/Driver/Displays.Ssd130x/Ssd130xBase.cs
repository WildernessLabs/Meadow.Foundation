using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Ssd130x
{
    /// <summary>
    /// Provide an interface to the SSD1306 family of OLED displays.
    /// </summary>
    public abstract partial class Ssd130xBase : DisplayBase
    {
        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override int Width => width;

        public override int Height => height;

        /// <summary>
        ///     SSD1306 SPI display
        /// </summary>
        protected ISpiPeripheral spiPeripheral;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ConnectionType connectionType;
        protected const bool Data = true;
        protected const bool Command = false;

        /// <summary>
        ///     SSD1306 I2C display
        /// </summary>
        protected II2cPeripheral i2cPeripheral;

        /// <summary>
        ///     Width of the display in pixels.
        /// </summary>
        protected int width;

        /// <summary>
        ///     Height of the display in pixels.
        /// </summary>
        protected int height;

        /// <summary>
        ///     X offset for non-standard displays.
        /// </summary>
        protected int xOffset = 0;

        /// <summary>
        ///     X offset for non-standard displays.
        /// </summary>
        protected int yOffset = 0;

        /// <summary>
        ///     Buffer holding the pixels in the display.
        /// </summary>
        protected Memory<byte> writeBuffer;
        protected Memory<byte> readBuffer;
        protected Memory<byte> commandBuffer;

        /// <summary>
        ///     Sequence of command bytes that must be sent to the display before
        ///     the Show method can send the data buffer.
        /// </summary>
        protected byte[] showPreamble;

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
        ///     Backing variable for the InvertDisplay property.
        /// </summary>
        private bool invertDisplay;

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
        ///     Backing variable for the Contrast property.
        /// </summary>
        private byte contrast;

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

        protected DisplayType displayType;

        /// <summary>
        ///     Send a command to the display.
        /// </summary>
        /// <param name="command">Command byte to send to the display.</param>
        private void SendCommand(byte command)
        {
            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort.State = Command;
                spiPeripheral.Write(command);
            }
            else
            {
                commandBuffer.Span[0] = 0x00;
                commandBuffer.Span[1] = command;
                i2cPeripheral.Exchange(commandBuffer.Span, readBuffer.Span[0..1]);
            }
        }

        /// <summary>
        ///     Send a sequence of commands to the display.
        /// </summary>
        /// <param name="commands">List of commands to send.</param>
        protected void SendCommands(Span<byte> commands)
        {
         //   var data = new byte[commands.Length + 1];
         //   data[0] = 0x00;
         //   Array.Copy(commands, 0, data, 1, commands.Length);

            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort.State = Command;
                spiPeripheral.Exchange(commands, readBuffer.Span);
            }
            else
            {
                i2cPeripheral.Write(0x00);
                i2cPeripheral.Exchange(commands, readBuffer.Span);
            }
        }

        /// <summary>
        ///     Send the internal pixel buffer to display.
        /// </summary>
        public override void Show()
        {
            SendCommands(showPreamble); //ToDo - pull this apart to do partial show

            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort.State = Data;
                spiPeripheral.Exchange(writeBuffer.Span, readBuffer.Span);
            }
            else //I2C
            {
                //
                //  Send the buffer page by page.
                //
                const int PAGE_SIZE = 16;

                for (int index = 0; index < writeBuffer.Length; index += PAGE_SIZE)
                {
                    if (writeBuffer.Length - index < PAGE_SIZE) { break; }

                    SendCommand(0x40);
                    i2cPeripheral.Exchange(writeBuffer.Span[index..(index + PAGE_SIZE)], readBuffer.Span);
                }
            }
        }

        public override void Show(int left, int top, int right, int bottom)
        {
            Show();
        }

        /// <summary>
        ///     Clear the display buffer.
        /// </summary>
        /// <param name="updateDisplay">Immediately update the display when true.</param>
        public override void Clear(bool updateDisplay = false)
        {
            Array.Clear(writeBuffer.Span.ToArray(), 0, writeBuffer.Length);

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        ///     Draw a pixel to the display - coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="color">Black - pixel off, any color - turn on pixel</param>
        public override void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
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
                {   //  pixels to be thrown away if out of bounds of the display
                    return;
                }
            }

            var index = (y >> 3) * width + x; //divide by 8

            if (colored)
            {
                writeBuffer.Span[index] = (byte)(writeBuffer.Span[index] | (byte)(1 << (y % 8)));
            }
            else
            {
                writeBuffer.Span[index] = (byte)(writeBuffer.Span[index] & ~(byte)(1 << (y % 8)));
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

            writeBuffer.Span[index] = (writeBuffer.Span[index] ^= (byte)(1 << y % 8));
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
                writeBuffer.Span[index] = (byte)(writeBuffer.Span[index] | (byte)(1 << (y % 8)));
            }
            else
            {
                writeBuffer.Span[index] = (byte)(writeBuffer.Span[index] & ~(byte)(1 << (y % 8)));
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