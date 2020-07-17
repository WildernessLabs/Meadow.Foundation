using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents APA102/Dotstar Led(s).
    /// </summary>
    /// <remarks>Based on logic from https://github.com/adafruit/Adafruit_CircuitPython_DotStar/blob/master/adafruit_dotstar.py </remarks>
    public class Apa102
    {
        //ToDo this could probably move into Meadow.Foundation.Core
        public enum PixelOrder
        {
            RGB,
            RBG,
            GRB,
            GBR,
            BRG,
            BGR
        }

        protected ISpiPeripheral spiPeripheral;

        const short StartHeaderSize = 4;
        const byte LedStart = 0b11100000;

        static readonly byte[] RGB = { 0, 1, 2 };
        static readonly byte[] RBG = { 0, 2, 1 };
        static readonly byte[] GRB = { 1, 0, 2 };
        static readonly byte[] GBR = { 1, 2, 0 };
        static readonly byte[] BRG = { 2, 0, 1 };
        static readonly byte[] BGR = { 2, 1, 0 };

        readonly uint numberOfLeds;
        readonly uint endHeaderSize;
        readonly byte[] buffer;
        readonly uint endHeaderIndex;
        readonly byte[] pixelOrder;
        public uint NumberOfLeds => numberOfLeds;

        public float Brightness 
        { 
            get => brightness;
            set 
            {
                if (value < 0)
                    brightness = 0;
                else if (value > 1f)
                    brightness = 1f;
                else
                    brightness = value;
            } 
        }

        public bool AutoWrite { get; set; }

        float brightness;

        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelect">THe SPI chip select pin. Not used but need for creating the  SPI Peripheral</param>
        /// <param name="numberOfLeds">The number of APA102 LEDs to control</param>
        /// <param name="pixelOrder">Set the pixel order on the LEDs - different strips implement this differently</param>
        /// <param name="autoWrite">Transmit any LED changes right away</param>
        public Apa102(ISpiBus spiBus, IDigitalOutputPort chipSelect, uint numberOfLeds, PixelOrder pixelOrder = PixelOrder.BGR, bool autoWrite = false)
        {

            spiPeripheral = new SpiPeripheral(spiBus, chipSelect);
            this.numberOfLeds = numberOfLeds;
            endHeaderSize = this.numberOfLeds / 16;
            Brightness = 1.0f;
            AutoWrite = autoWrite;

            if (this.numberOfLeds % 16 != 0)
            {
                endHeaderSize += 1;
            }

            buffer = new byte[this.numberOfLeds * 4 + StartHeaderSize + endHeaderSize];
            endHeaderIndex = (uint)(buffer.Length - endHeaderSize);

            switch (pixelOrder)
            {
                case PixelOrder.RGB:
                    this.pixelOrder = RGB;
                    break;
                case PixelOrder.RBG:
                    this.pixelOrder = RBG;
                    break;
                case PixelOrder.GRB:
                    this.pixelOrder = GRB;
                    break;
                case PixelOrder.GBR:
                    this.pixelOrder = GBR;
                    break;
                case PixelOrder.BRG:
                    this.pixelOrder = BRG;
                    break;
                case PixelOrder.BGR:
                    this.pixelOrder = BGR;
                    break;
            }

            for (int i = 0; i < StartHeaderSize; i++)
            {
                buffer[i] = 0x00;
            }

            for (int i = StartHeaderSize; i < endHeaderIndex; i += 4)
            {
                buffer[i] = 0xFF;
            }

            for (uint i = endHeaderIndex; i < buffer.Length; i++)
            {
                buffer[i] = 0xFF;
            }
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="color">The color</param>
        public virtual void SetLed(uint index, Color color)
        {
            SetLed(index, color, Brightness);
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="color">The color</param>
        /// <param name="brightness">The brighrness 0.0 - 1.0f</param>
        public virtual void SetLed(uint index, Color color, float brightness = 1f)
        {
            byte[] bColor = new byte[] { (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255) };
            SetLed(index, bColor, brightness);
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="rgb">Byte array representing the color RGB values. byte[0] = Red, byte[1] = Green, byte[2] = Blue</param>
        public virtual void SetLed(uint index, byte[] rgb)
        {
            SetLed(index, rgb, Brightness);
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="rgb">Byte array representing the color RGB values. byte[0] = Red, byte[1] = Green, byte[2] = Blue</param>
        /// <param name="brightness">The brighrness 0.0 - 1.0f</param>
        public virtual void SetLed(uint index, byte[] rgb, float brightness = 1f)
        {
            if (index > numberOfLeds)
            {
                throw new ArgumentOutOfRangeException("Index must be less than the number of leds specified");
            }

            if (brightness < 0 || brightness > 1f)
            {
                throw new ArgumentOutOfRangeException("brightness must be between 0.0 and 1.0");
            }

            var offset = index * 4 + StartHeaderSize;
            //var rgb = value;
            byte brightnessByte;

            //rgb[0] = value[0];
            //rgb[1] = value[1];
            //rgb[2] = value[2];

            brightnessByte = (byte)(32 - (32 - (int)(brightness * 31)) & 0b00011111);
            buffer[offset] = (byte)(brightnessByte | LedStart);
            buffer[offset + 1] = rgb[pixelOrder[0]];
            buffer[offset + 2] = rgb[pixelOrder[1]];
            buffer[offset + 3] = rgb[pixelOrder[2]];

            if (AutoWrite)
            {
                Show();
            }
        }

        /// <summary>
        /// Turn off all the Leds
        /// </summary>
        public virtual void Clear(bool autoWrite = false)
        {
            byte[] off = {0, 0, 0};

            for(uint i=0; i< NumberOfLeds; i++)
            {
                SetLed(i, off);
            }

            if (!AutoWrite && autoWrite)
            {
                Show();
            }
        }

        /// <summary>
        /// Transmit the changes to the LEDs 
        /// </summary>
        public virtual void Show()
        {
            spiPeripheral.WriteBytes(buffer);
        }
    }
}