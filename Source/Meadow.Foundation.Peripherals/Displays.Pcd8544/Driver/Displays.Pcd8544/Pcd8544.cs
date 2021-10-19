using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using System;
using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays
{
    public class Pcd8544 : DisplayBase
    {
        public static int DEFAULT_SPEED = 4000;

        public override ColorType ColorMode => ColorType.Format1bpp;

        public override int Height => 48;

        public override int Width => 84;

        public bool IsDisplayInverted { get; private set; } = false;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ISpiPeripheral spiDisplay;

        protected Buffer1 imageBuffer;

        protected Memory<byte> commandBuffer;

        public Pcd8544(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin)
        {
            imageBuffer = new Buffer1(Width, Height);
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
        public override void Clear(bool updateDisplay = false)
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
        public override void DrawPixel(int x, int y, bool colored)
        {
            imageBuffer.SetPixel(x, y, colored);
        }

        public override void InvertPixel(int x, int y)
        {
            imageBuffer.SetPixel(x, y, !imageBuffer.GetPixelBool(x, y));
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="color">any value other than black will make the pixel visible</param>
        public override void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        public override void Show()
        {
            spiDisplay.Write(imageBuffer.Buffer);
        }

        public override void Show(int left, int top, int right, int bottom)
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

        public override void Clear(Color clearColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(clearColor);

            if(updateDisplay) { Show(); }
        }

        public override void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }
    }
}