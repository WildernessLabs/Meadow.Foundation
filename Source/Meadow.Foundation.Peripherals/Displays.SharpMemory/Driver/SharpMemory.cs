using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Graphics;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface to the SharpMemory family of displays.
    /// </summary>
    public partial class SharpMemory : IGraphicsDisplay
    {
        /// <summary>
        /// Display color depth (1bpp)
        /// </summary>
        public ColorType ColorMode => ColorType.Format1bpp;

        /// <summary>
        /// Width of display in pixels
        /// </summary>
        public int Width => imageBuffer.Width;

        /// <summary>
        /// Height of display in pixels
        /// </summary>
        public int Height => imageBuffer.Height;

        /// <summary>
        /// Ignore out of bounds pixels
        /// </summary>
        public bool IgnoreOutOfBoundsPixels { get; set; }

        const byte SHARPMEM_BIT_WRITECMD = 0x01; // 0x80 in LSB format
        const byte SHARPMEM_BIT_VCOM = 0x02;     // 0x40 in LSB format
        const byte SHARPMEM_BIT_CLEAR = 0x04;    // 0x20 in LSB format

        /// <summary>
        /// SPI perihperal object
        /// </summary>
        protected ISpiPeripheral spiPerihperal;

        /// <summary>
        /// Port connected to chip select pin
        /// </summary>
        protected IDigitalOutputPort chipSelectPort;

        readonly Buffer1bpp imageBuffer;

        byte[] lineBuffer;

        byte vcom = 0;

        /// <summary>
        /// Create a new SharpMemory display object 
        /// </summary>
        public SharpMemory(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, 
            int width = 144, int height = 168)
        {
            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin, false);

            spiPerihperal = new SpiPeripheral(spiBus, chipSelectPort, csMode: ChipSelectMode.ActiveHigh);
            //spiPerihperal = new SpiPeripheral()


            imageBuffer = new Buffer1bpp(width, height);

            vcom = 0;

            /*
            _ = Task.Run(() =>
            {
                while (true)
                {
                    ToggleVCom();
                    Thread.Sleep(900);
                }
            });
            */
        }

        void ToggleVCom()
        {
            vcom = (byte)((vcom == SHARPMEM_BIT_VCOM) ? 0x00 : SHARPMEM_BIT_VCOM);
        }

        /// <summary>
        /// Send the internal pixel buffer to display.
        /// </summary>
        public void Show()
        {
            var bytesPerLine = Width / 8;

            if (lineBuffer == null)
            {
                lineBuffer = new byte[bytesPerLine + 2];
            }

            chipSelectPort.State = true;

            //spiBus.Write(null, new byte[] { (byte)(vcom | SHARPMEM_BIT_WRITECMD) });

            spiPerihperal.Write(new byte[] { (byte)(vcom | SHARPMEM_BIT_WRITECMD) });

            for (int i = 0; i < imageBuffer.ByteCount; i += bytesPerLine)
            {
                lineBuffer[0] = (byte)((i + 1) / bytesPerLine + 1);
                Array.Copy(imageBuffer.Buffer, i, lineBuffer, 1, bytesPerLine);
                lineBuffer[bytesPerLine + 1] = 0;

                //spiBus.Write(null, lineBuffer);
                spiPerihperal.Write(lineBuffer);
            }

            //spiBus.Write(null, new byte[] { 0 });
            spiPerihperal.Write(new byte[] { 0 });

            chipSelectPort.State = false;

            spiPerihperal.Write(new byte[] { vcom, 0 });

        }

        public void Show(int left, int top, int right, int bottom)
        {
            Show();
        }

        /// <summary>
        /// Clear the display buffer.
        /// </summary>
        /// <param name="updateDisplay">Immediately update the display when true.</param>
        public void Clear(bool updateDisplay = false)
        {
            imageBuffer.Clear();

            if (updateDisplay) 
            {
                spiPerihperal.Write(new byte[] { (byte)(vcom | SHARPMEM_BIT_CLEAR), 0x0 });
                //Show();
            }
        }

        /// <summary>
        /// Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="color">Any color = turn on pixel, black = turn off pixel</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        /// Coordinates start with index 0
        /// </summary>
        /// <param name="x">x location of pixel</param>
        /// <param name="y">y location of pixel</param>
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

        /// <summary>
        /// Invert a pixel
        /// </summary>
        /// <param name="x">x location of pixel</param>
        /// <param name="y">y location of pixel</param>
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