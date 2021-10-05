using Meadow.Devices;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Displays
{
    public class Pcd8544 : DisplayBase
    {
        public static int DEFAULT_SPEED = 4000;

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override int Height => 48;

        public override int Width => 84;

        public bool IsDisplayInverted { get; private set; } = false;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ISpiPeripheral spiDisplay;

        protected byte[] writeBuffer;
        protected byte[] readBuffer;
        protected Memory<byte> commandBuffer;

        public Pcd8544(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin)
        {
            writeBuffer = new byte[Width * Height / 8];
            readBuffer = new byte[Width * Height / 8];
            commandBuffer = new byte[1];

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

            writeBuffer[0] = 0x21;
            writeBuffer[1] = 0xBF;
            writeBuffer[2] = 0x04;
            writeBuffer[3] = 0x14;
            writeBuffer[4] = 0x0D;
            writeBuffer[5] = 0x20;
            writeBuffer[6] = 0x0C;

            spiDisplay.Exchange(writeBuffer[0..6], readBuffer[0..6]);

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
            Array.Clear(writeBuffer, 0, writeBuffer.Length);

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
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            { return; } // out of the range! return true to indicate failure.

            ushort index = (ushort)((x % 84) + (int)(y * 0.125) * 84);

            byte bitMask = (byte)(1 << (y % 8));

            if (colored)
            {
                writeBuffer[index] |= bitMask;
            }
            else
            {
                writeBuffer[index] &= (byte)~bitMask;
            }
        }

        public override void InvertPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            { return; } // out of the range! return true to indicate failure.

            ushort index = (ushort)((x % 84) + (int)(y * 0.125) * 84);

            byte bitMask = (byte)(1 << (y % 8));

            writeBuffer[index] = (writeBuffer[index] ^= bitMask);
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
            spiDisplay.Exchange(writeBuffer, readBuffer);
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

            spiDisplay.Exchange(commandBuffer.Span, readBuffer[0..0]);
            dataCommandPort.State = (true);
        }
    }
}