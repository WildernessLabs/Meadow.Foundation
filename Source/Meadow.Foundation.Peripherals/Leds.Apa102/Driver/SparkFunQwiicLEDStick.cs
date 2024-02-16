using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a SparkFunQwiicLEDStick that uses APA102 leds
    /// </summary>
    public partial class SparkFunQwiicLEDStick : I2cCommunications, IApa102
    {
        /// <summary>
        /// Enable or disable autowrite to update the LEDs as they're set
        /// </summary>
        public bool AutoWrite { get; set; }

        float brightness;
        byte[][]? buffer;

        const int ArrayLength = 10;

        /// <summary>
        /// Creates a new SparkFunQwiicLEDStick object
        /// </summary>
        /// <param name="i2cbus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public SparkFunQwiicLEDStick(II2cBus i2cbus,
            byte address = (byte)Addresses.Default)
            : base(i2cbus, address, 16)
        {
            Initialize();
        }

        /// <summary>
        /// The led brightness (0-1)
        /// </summary>
        public float Brightness
        {
            get => brightness;
            set
            {
                if (value == Brightness) return;

                if (value < 0) { brightness = 0; }
                else if (value > 1f) { brightness = 1f; }
                else { brightness = value; }
            }
        }

        void Initialize()
        {
            SetLength(ArrayLength);
            Brightness = 1f;
            AutoWrite = false;

            Clear();
        }

        byte ConvertBrightness(float b)
        {   // the internal brightness is 0-31, so scale from the float interface property
            var result = (byte)(b * 31f);
            if (result < 0) return 0;
            if (result > 31) return 31;
            return result;
        }

        void SetLength(byte length)
        {
            buffer = new byte[4][]
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
        /// <param name="color">The led color</param>
        /// <param name="brightness">The led brightness (0-1)</param>
        public void All(Color color, float brightness = 1f)
        {
            var bright = ConvertBrightness(brightness);

            for (byte b = 0; b < ArrayLength; b++)
            {
                buffer![0][b] = color.R;
                buffer[1][b] = color.G;
                buffer[2][b] = color.B;
                buffer[3][b] = bright;
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
        /// <param name="brightness">The brightness 0.0 - 1.0f</param>
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
        /// <param name="brightness">The brightness 0.0 - 1.0f</param>
        public virtual void SetLed(int index, byte[] rgb, float brightness = 1f)
        {
            if (rgb.Length % 3 != 0)
            {
                throw new ArgumentException("Data length must be a multiple of 3 (RGB sets).");
            }

            var requestedBrightness = ConvertBrightness(brightness);
            var setColor = false;
            var setBrightness = false;

            if (buffer![0][index] != rgb[0]) setColor = true;
            if (buffer[1][index] != rgb[1]) setColor = true;
            if (buffer[2][index] != rgb[2]) setColor = true;
            if (buffer[3][index] != requestedBrightness) setBrightness = true;

            buffer[0][index] = rgb[0];
            buffer[1][index] = rgb[1];
            buffer[2][index] = rgb[2];
            buffer[3][index] = requestedBrightness;

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

        void TransmitData()
        {
            // RED
            var r = new byte[buffer![0].Length + 3];
            r[0] = (byte)Register.RedArray;
            r[1] = (byte)(buffer[0].Length);
            r[2] = 0;
            Array.Copy(buffer[0], 0, r, 3, buffer[0].Length);
            base.Write(r);

            //// GREEN
            var g = new byte[buffer[1].Length + 3];
            g[0] = (byte)Register.GreenArray;
            g[1] = (byte)(buffer[1].Length);
            g[2] = 0;
            Array.Copy(buffer[1], 0, g, 3, buffer[1].Length);
            base.Write(g);

            // BLUE
            var b = new byte[buffer[2].Length + 3];
            b[0] = (byte)Register.BlueArray;
            b[1] = (byte)(buffer[2].Length);
            b[2] = 0;
            Array.Copy(buffer[2], 0, b, 3, buffer[2].Length);
            base.Write(b);

            WriteBrightnessArray();
        }

        void WriteBrightnessArray()
        {   // TODO: this could be improved - need to track current brightness and only write on change
            for (byte b = 0; b < ArrayLength; b++)
            {
                base.WriteRegister((byte)Register.SingleBrightness, new byte[] { b, buffer![3][b] });
            }
        }

        /// <summary>
        /// Turn off all the Leds
        /// </summary>
        public void Clear(bool update = false)
        {
            Array.Clear(buffer![0], 0, buffer[0].Length);
            Array.Clear(buffer[1], 0, buffer[0].Length);
            Array.Clear(buffer[2], 0, buffer[0].Length);
            Array.Clear(buffer[3], 0, buffer[0].Length);

            base.Write((byte)Register.AllOff);
        }
    }
}