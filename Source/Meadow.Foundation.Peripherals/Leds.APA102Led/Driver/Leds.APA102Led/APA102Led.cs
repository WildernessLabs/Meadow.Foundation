using Meadow.Hardware;
using System;
using System.Security.Policy;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents APA102/Dotstar Led(s).
    /// </summary>
    /// <remarks>Based on logic from https://github.com/adafruit/Adafruit_CircuitPython_DotStar/blob/master/adafruit_dotstar.py </remarks>
    public class APA102Led
    {
        public enum PixelOrder
        {
            RGB,
            RBG,
            GRB,
            GBR,
            BRG,
            BGR
        }

        protected ISpiPeripheral _spiPeriph;

        const short StartHeaderSize = 4;
        const byte LedStart = 0b11100000;

        static readonly byte[] RGB = { 0, 1, 2 };
        static readonly byte[] RBG = { 0, 2, 1 };
        static readonly byte[] GRB = { 1, 0, 2 };
        static readonly byte[] GBR = { 1, 2, 0 };
        static readonly byte[] BRG = { 2, 0, 1 };
        static readonly byte[] BGR = { 2, 1, 0 };

        readonly uint _numberOfLeds;
        readonly uint _endHeaderSize;
        readonly byte[] _buffer;
        readonly uint _endHeaderIndex;
        readonly byte[] _pixelOrder;
        public uint NumberOfLeds => _numberOfLeds;

        public float Brightness 
        { 
            get => _brightness;
            set 
            {
                if (value < 0)
                    _brightness = 0;
                else if (value > 1f)
                    _brightness = 1f;
                else
                    _brightness = value;
            } 
        }

        public bool AutoWrite { get; set; }

        float _brightness;

        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelect">THe SPI chip select pin. Not used but need for creating the  SPI Peripheral</param>
        /// <param name="numberOfLeds">The number of APA102 LEDs to control</param>
        /// <param name="pixelOrder">Set the pixel order on the LEDs - different strips implement this differently</param>
        /// <param name="autoWrite">Transmit any LED changes right away</param>
        public APA102Led(ISpiBus spiBus, IDigitalOutputPort chipSelect, uint numberOfLeds, PixelOrder pixelOrder = PixelOrder.BGR, bool autoWrite = false)
        {

            _spiPeriph = new SpiPeripheral(spiBus, chipSelect);
            _numberOfLeds = numberOfLeds;
            _endHeaderSize = _numberOfLeds / 16;
            Brightness = 1.0f;
            AutoWrite = autoWrite;

            if (_numberOfLeds % 16 != 0)
                _endHeaderSize += 1;

            _buffer = new byte[_numberOfLeds * 4 + StartHeaderSize + _endHeaderSize];
            _endHeaderIndex = (uint)(_buffer.Length - _endHeaderSize);

            switch (pixelOrder)
            {
                case PixelOrder.RGB:
                    this._pixelOrder = RGB;
                    break;
                case PixelOrder.RBG:
                    this._pixelOrder = RBG;
                    break;
                case PixelOrder.GRB:
                    this._pixelOrder = GRB;
                    break;
                case PixelOrder.GBR:
                    this._pixelOrder = GBR;
                    break;
                case PixelOrder.BRG:
                    this._pixelOrder = BRG;
                    break;
                case PixelOrder.BGR:
                    this._pixelOrder = BGR;
                    break;
            }

            for (int i = 0; i < StartHeaderSize; i++)
                _buffer[i] = 0x00;


            for (int i = StartHeaderSize; i < _endHeaderIndex; i += 4)
                _buffer[i] = 0xFF;

            for (uint i = _endHeaderIndex; i < _buffer.Length; i++)
                _buffer[i] = 0xFF;


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
            if (index > _numberOfLeds)
                throw new ArgumentOutOfRangeException("Index must be less than the number of leds specified");

            if (brightness < 0 || brightness > 1f)
                throw new ArgumentOutOfRangeException("brightness must be between 0.0 and 1.0");

            var offset = index * 4 + StartHeaderSize;
            //var rgb = value;
            byte brightnessByte;

            //rgb[0] = value[0];
            //rgb[1] = value[1];
            //rgb[2] = value[2];

            brightnessByte = (byte)(32 - (32 - (int)(brightness * 31)) & 0b00011111);
            _buffer[offset] = (byte)(brightnessByte | LedStart);
            _buffer[offset + 1] = rgb[_pixelOrder[0]];
            _buffer[offset + 2] = rgb[_pixelOrder[1]];
            _buffer[offset + 3] = rgb[_pixelOrder[2]];

            if (AutoWrite)
                Show();
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
                Show();
        }

        /// <summary>
        /// Transmit the changes to the LEDs 
        /// </summary>
        public virtual void Show()
        {
            _spiPeriph.WriteBytes(_buffer);
        }

    }
}
