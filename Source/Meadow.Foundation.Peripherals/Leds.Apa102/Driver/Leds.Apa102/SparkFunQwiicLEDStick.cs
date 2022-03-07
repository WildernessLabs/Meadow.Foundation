using Meadow.Hardware;

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
            : base(bus, address)
        {
        }

        public float Brightness
        {
            get => _brightness;
            set
            {
                if (value < 0) { _brightness = 0; }
                else if (value > 1f) { _brightness = 1f; }
                else { _brightness = value; }
            }
        }

        private void Initialize()
        {
            Brightness = 1f;
            SetLength(ArrayLength);

            Clear();
        }

        private byte ConvertBrightness(float b)
        {
            // the internal brightness is 0-31, so scale from the float interface property
            return (byte)(b * 31f);
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
            _buffer[0][index] = rgb[0];
            _buffer[1][index] = rgb[1];
            _buffer[2][index] = rgb[2];
            _buffer[3][index] = ConvertBrightness(brightness);
        }

        /// <summary>
        /// Transmit the changes to the LEDs 
        /// </summary>
        public void Show()
        {
            base.WriteRegisters((byte)Register.RedArray, _buffer[0]);
            base.WriteRegisters((byte)Register.GreenArray, _buffer[1]);
            base.WriteRegisters((byte)Register.BlueArray, _buffer[2]);

            WriteBrightnessArray();
        }

        private void WriteBrightnessArray()
        {
            // TODO: this could be improved - need to track current brightness and only write on change
            for (byte b = 0; b < ArrayLength; b++)
            {
                base.WriteRegisters((byte)Register.SingleBrightness, new byte[] { b, _buffer[3][b] });
            }
        }

        /// <summary>
        /// Turn off all the Leds
        /// </summary>
        public void Clear(bool update = false)
        {
            base.Write((byte)Register.AllOff);
        }
    }
}