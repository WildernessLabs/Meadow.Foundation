using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using System;
using Meadow.Foundation.Graphics;
using Meadow.Units;

namespace Meadow.Foundation.Displays
{
    public class Pcd8544 : IGraphicsDisplay
    {
        public static Frequency DEFAULT_SPEED = new Frequency(4000, Frequency.UnitType.Kilohertz);

        public ColorType ColorMode => ColorType.Format1bpp;

        public int Height => 48;

        public int Width => 84;

        public bool IgnoreOutOfBoundsPixels { get; set; }

        public bool IsDisplayInverted { get; private set; } = false;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ISpiPeripheral spiDisplay;

        protected Buffer1bpp imageBuffer;

        protected Memory<byte> commandBuffer;

        public Pcd8544(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin)
        {
            imageBuffer = new Buffer1bpp(Width, Height);
            commandBuffer = new byte[7];

            dataCommandPort = device.CreateDigitalOutputPort(dcPin, true);
            resetPort = device.CreateDigitalOutputPort(resetPin, true);
            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin);

            spiDisplay = new SpiPeripheral(spiBus, chipSelectPort);

            Initialize();
        }

        private void Initialize()
        {
            resetPort.State = false;
            resetPort.State = true;

            dataCommandPort.State = false;

            commandBuffer.Span[0] = 0x21;
            commandBuffer.Span[1] = 0xBF;
            commandBuffer.Span[2] = 0x04;
            commandBuffer.Span[3] = 0x14;
            commandBuffer.Span[4] = 0x0D;
            commandBuffer.Span[5] = 0x20;
            commandBuffer.Span[6] = 0x0C;

            spiDisplay.Write(commandBuffer.Span[0..6]);

            dataCommandPort.State = true;

            Clear();
            Show();
        }

        /// <summary>
        ///     Clear the display
        /// </summary>
        /// <remarks>
        ///     Clears the internal memory buffer 
        /// </remarks>
        /// <param name="updateDisplay">If true, it will force a display update</param>
        public void Clear(bool updateDisplay = false)
        {
            Array.Clear(imageBuffer.Buffer, 0, imageBuffer.ByteCount);

            if (updateDisplay)
            {
                Show();
            }
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

            imageBuffer.SetPixel(x, y, !imageBuffer.GetPixelIsColored(x, y));
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="color">any value other than black will make the pixel visible</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        public void Show()
        {
            spiDisplay.Write(imageBuffer.Buffer);
        }

        public void Show(int left, int top, int right, int bottom)
        {
            //ToDo implement partial screen updates for PCD8544
            Show();
        }

        public void InvertDisplay(bool inverse)
        {
            IsDisplayInverted = inverse;
            dataCommandPort.State = false;
            commandBuffer.Span[0] = inverse ? (byte)0x0D : (byte)0x0C;

            spiDisplay.Write(commandBuffer.Span[0]);
            dataCommandPort.State = true;
        }

        public void Fill(Color clearColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(clearColor.Color1bpp);

            if(updateDisplay) { Show(); }
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