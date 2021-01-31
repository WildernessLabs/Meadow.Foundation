using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    public class Pcd8544 : DisplayBase
    {
        public static int DEFAULT_SPEED = 4000;

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override int Height => 48;

        public override int Width => 84;

        public bool InvertDisplay
        {
            get { return _invertDisplay; }
            set { Invert(value); }
        }
        protected bool _invertDisplay = false;

        protected Color currentPen = Color.White;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ISpiPeripheral spiDisplay;
        protected SpiBus spi;

        protected byte[] displayBuffer;
        protected readonly byte[] spiReceive;

        public Pcd8544(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin)
        {
            displayBuffer = new byte[Width * Height / 8];
            spiReceive = new byte[Width * Height / 8];

            dataCommandPort = device.CreateDigitalOutputPort(dcPin, true);
            resetPort = device.CreateDigitalOutputPort(resetPin, true);
            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin);

            spi = (SpiBus)spiBus;
            spiDisplay = new SpiPeripheral(spiBus, chipSelectPort);

            Initialize();
        }

        private void Initialize()
        {
            resetPort.State = (false);
            resetPort.State = (true);

            dataCommandPort.State = (false);

            spiDisplay.WriteBytes(new byte[]
            {
                0x21, // LCD Extended Commands.
                0xBF, // Set LCD Vop (Contrast). //0xB0 for 5V, 0XB1 for 3.3v, 0XBF if screen too dark
                0x04, // Set Temp coefficient. //0x04
                0x14, // LCD bias mode 1:48. //0x13 or 0X14
                0x0D, // LCD in normal mode. 0x0d for inverse
                0x20, // We must send 0x20 before modifying the display control mode
                0x0C // Set display control, normal mode. 0x0D for inverse, 0x0C for normal
            });

            dataCommandPort.State = (true);

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
            displayBuffer = new byte[Width * Height / 8];

            for(int i = 0; i < displayBuffer.Length; i++)
            {
                displayBuffer[i] = 0;
            }

            if(updateDisplay)
            {
                Show();
            }
        }

        public override void SetPenColor(Color pen)
        {
            currentPen = pen;
        }

        /// <summary>
        ///     Coordinates start with index 0
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
        /// <param name="colored">True = turn on pixel, false = turn off pixel</param>
        public override void DrawPixel(int x, int y, bool colored)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            { return; } // out of the range! return true to indicate failure.

            ushort index = (ushort)((x % 84) + (int)(y * 0.125) * 84);

            byte bitMask = (byte)(1 << (y % 8));

            if (colored)
            {
                displayBuffer[index] |= bitMask;
            }
            else
            {
                displayBuffer[index] &= (byte)~bitMask;
            }
        }

        public override void InvertPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            { return; } // out of the range! return true to indicate failure.

            ushort index = (ushort)((x % 84) + (int)(y * 0.125) * 84);

            byte bitMask = (byte)(1 << (y % 8));

            displayBuffer[index] = (displayBuffer[index] ^= bitMask);
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="color">any value other than black will make the pixel visible</param>
        public override void DrawPixel(int x, int y, Color color)
        {
            var colored = color != Color.Black;

            DrawPixel(x, y, colored);
        }

        public override void Show()
        {
          //  spiDisplay.WriteBytes(spiBuffer);

            spi.ExchangeData(chipSelectPort, ChipSelectMode.ActiveLow, displayBuffer, spiReceive);
        }

        private void Invert(bool inverse)
        {
            _invertDisplay = inverse;
            dataCommandPort.State = (false);
            spiDisplay.WriteBytes(inverse ? new byte[] { 0x0D } : new byte[] { 0x0C });
            dataCommandPort.State = (true);
        }
    }
}