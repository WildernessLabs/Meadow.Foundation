using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Leds
{
    public partial class SparkFunQwiicLEDStick : I2cPeripheral, IApa102
    {
        private float _brightness;
        private byte[][] _buffer;

        public bool AutoWrite { get; set; }

        private const int ArrayLength = 10;

        public SparkFunQwiicLEDStick(II2cBus bus,
            byte address = (byte)Addresses.Default)
            : base(bus, address, 1, 16)
        {
            Initialize();
        }

        public float Brightness
        {
            get => _brightness;
            set
            {
                if (value == Brightness) return;

                if (value < 0) { _brightness = 0; }
                else if (value > 1f) { _brightness = 1f; }
                else { _brightness = value; }
            }
        }

        private void Initialize()
        {
            SetLength(ArrayLength);
            Brightness = 1f;
            AutoWrite = false;

            Clear();
        }

        private byte ConvertBrightness(float b)
        {
            // the internal brightness is 0-31, so scale from the float interface property
            var result = (byte)(b * 31f);
            if (result < 0) return 0;
            if (result > 31) return 31;
            return result;
        }

        private void SetLength(byte length)
        {
            _buffer = new byte[4][]
            {
                new byte[length],
                new byte[length],
                new byte[length],
                new byte[length]
            };

            WriteRegister((byte)Register.LedLength, length);
        }

        /// <summary>
        /// Sets all LEDs to a given color
        /// </summary>
        /// <param name="color"></param>
        public void All(Color color, float brightness = 1f)
        {
            var bright = ConvertBrightness(brightness);

            for (byte b = 0; b < ArrayLength; b++)
            {
                _buffer[0][b] = color.R;
                _buffer[1][b] = color.G;
                _buffer[2][b] = color.B;
                _buffer[3][b] = bright;
            }

            WriteRegister((byte)Register.AllColor, new byte[] { color.R, color.G, color.B });
            WriteRegister((byte)Register.AllBrightness, bright);
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="color">The color</param>
        public virtual void SetLed(int index, Color color)
        {
            SetLed(index, color, Brightness);
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="color">The color</param>
        /// <param name="brightness">The brighrness 0.0 - 1.0f</param>
        public virtual void SetLed(int index, Color color, float brightness = 1f)
        {
            SetLed(index, new byte[] { color.R, color.G, color.B }, brightness);
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="rgb">Byte array representing the color RGB values. byte[0] = Red, byte[1] = Green, byte[2] = Blue</param>
        public virtual void SetLed(int index, byte[] rgb)
        {
            SetLed(index, rgb, Brightness);
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="rgb">Byte array representing the color RGB values. byte[0] = Red, byte[1] = Green, byte[2] = Blue</param>
        /// <param name="brightness">The brighrness 0.0 - 1.0f</param>
        public virtual void SetLed(int index, byte[] rgb, float brightness = 1f)
        {
            if (rgb.Length % 3 != 0)
            {
                throw new ArgumentException("Data length must be a multiple of 3 (RGB sets).");
            }

            var requestedBrightness = ConvertBrightness(brightness);
            var setColor = false;
            var setBrightness = false;

            if (_buffer[0][index] != rgb[0]) setColor = true;
            if (_buffer[1][index] != rgb[1]) setColor = true;
            if (_buffer[2][index] != rgb[2]) setColor = true;
            if (_buffer[3][index] != requestedBrightness) setBrightness = true;

            _buffer[0][index] = rgb[0];
            _buffer[1][index] = rgb[1];
            _buffer[2][index] = rgb[2];
            _buffer[3][index] = requestedBrightness;

            if (AutoWrite)
            {
                if (rgb.Length == 3)
                {
                    // one pixel only - only make required updates
                    if (setColor)
                    {
                        WriteRegister((byte)Register.SingleColor, new byte[] { (byte)index, rgb[0], rgb[1], rgb[2] });
                    }
                    if (setBrightness)
                    {
                        WriteRegister((byte)Register.SingleBrightness, new byte[] { (byte)index, ConvertBrightness(brightness) });
                    }
                }
                else
                {
                    TransmitData();
                }
            }
        }

        /// <summary>
        /// Transmit the changes to the LEDs 
        /// </summary>
        public void Show()
        {
            if (AutoWrite) return;

            TransmitData();
        }

        private void TransmitData()
        {
            // RED
            var r = new byte[_buffer[0].Length + 3];
            r[0] = (byte)Register.RedArray;
            r[1] = (byte)(_buffer[0].Length);
            r[2] = 0;
            Array.Copy(_buffer[0], 0, r, 3, _buffer[0].Length);
            base.Write(r);

            //// GREEN
            var g = new byte[_buffer[1].Length + 3];
            g[0] = (byte)Register.GreenArray;
            g[1] = (byte)(_buffer[1].Length);
            g[2] = 0;
            Array.Copy(_buffer[1], 0, g, 3, _buffer[1].Length);
            base.Write(g);

            // BLUE
            var b = new byte[_buffer[2].Length + 3];
            b[0] = (byte)Register.BlueArray;
            b[1] = (byte)(_buffer[2].Length);
            b[2] = 0;
            Array.Copy(_buffer[2], 0, b, 3, _buffer[2].Length);
            base.Write(b);

            WriteBrightnessArray();
        }

        private void WriteBrightnessArray()
        {
            // TODO: this could be improved - need to track current brightness and only write on change
            for (byte b = 0; b < ArrayLength; b++)
            {
                base.WriteRegister((byte)Register.SingleBrightness, new byte[] { b, _buffer[3][b] });
            }
        }

        /// <summary>
        /// Turn off all the Leds
        /// </summary>
        public void Clear(bool update = false)
        {
            Array.Clear(_buffer[0], 0, _buffer[0].Length);
            Array.Clear(_buffer[1], 0, _buffer[0].Length);
            Array.Clear(_buffer[2], 0, _buffer[0].Length);
            Array.Clear(_buffer[3], 0, _buffer[0].Length);

            base.Write((byte)Register.AllOff);
        }
    }
}