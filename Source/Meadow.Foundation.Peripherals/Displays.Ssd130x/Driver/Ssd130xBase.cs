using System;
using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays.Ssd130x
{
    /// <summary>
    /// Provide an interface to the SSD1306 family of OLED displays.
    /// </summary>
    public abstract partial class Ssd130xBase : IGraphicsDisplay
    {
        public ColorType ColorMode => ColorType.Format1bpp;

        public int Width => imageBuffer.Width;

        public int Height => imageBuffer.Height;

        public bool IgnoreOutOfBoundsPixels { get; set; }

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

        protected const int PAGE_SIZE = 16;

        /// <summary>
        ///     SSD1306 I2C display
        /// </summary>
        protected II2cPeripheral i2cPeripheral;

        /// <summary>
        ///     Buffer holding the pixels in the display.
        /// </summary>
        protected Buffer1bpp imageBuffer;
        protected byte[] readBuffer;
        protected Memory<byte> commandBuffer;
        protected byte[] pageBuffer;

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
                i2cPeripheral.Write(commandBuffer.Span);
            }
        }

        /// <summary>
        ///     Send a sequence of commands to the display.
        /// </summary>
        /// <param name="commands">List of commands to send.</param>
        protected void SendCommands(Span<byte> commands)
        {
            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort.State = Command;
                spiPeripheral.Write(commands);
            }
            else
            {   //a little heavy but this is only used a couple of times
                //we can optimize when we switch writeBuffer to Memory<byte>
                Span<byte> data = new byte[commands.Length + 1];
                data[0] = 0x00;
                commands.CopyTo(data.Slice(1, commands.Length));
                i2cPeripheral.Write(data);
            }
        }

        /// <summary>
        ///     Send the internal pixel buffer to display.
        /// </summary>
        public void Show()
        {
            SendCommands(showPreamble); //ToDo - pull this apart to do partial show

            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort.State = Data;
                spiPeripheral.Bus.Exchange(chipSelectPort, imageBuffer.Buffer, readBuffer);
               // spiPeripheral.Write(imageBuffer.Buffer); //happy path
            }
            else//  I2C
            {   //  Send the buffer page by page
                //  This can be optimized when we move to Memory<byte>
                pageBuffer[0] = 0x40;

                for (ushort index = 0; index < imageBuffer.ByteCount; index += PAGE_SIZE)
                {
                    if (imageBuffer.ByteCount - index < PAGE_SIZE) { break; }

                    Array.Copy(imageBuffer.Buffer, index, pageBuffer, 1, PAGE_SIZE);
                    i2cPeripheral.Write(pageBuffer);
                }
            }
        }

        public void Show(int left, int top, int right, int bottom)
        {
            Show();
        }

        /// <summary>
        ///     Clear the display buffer.
        /// </summary>
        /// <param name="updateDisplay">Immediately update the display when true.</param>
        public void Clear(bool updateDisplay = false)
        {
            Array.Clear(imageBuffer.Buffer, 0, imageBuffer.ByteCount);

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
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        ///     Draw a pixel to the display - coordinates start with index 0
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
            if(IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            var index = (y >> 8) * Width + x;

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
                commands = new byte[]
                    { 0xa3, 0x00, (byte) Height, scrollDirection, 0x00, startPage, 0x00, endPage, 0x01, 0x2f };
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

        public virtual void Fill(Color fillColor, bool updateDisplay = false)
        {
            throw new NotImplementedException();
        }

        public virtual void Fill(int x, int y, int width, int height, Color fillColor)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {
            throw new NotImplementedException();
        }
    }
}