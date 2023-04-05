using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an Ht16k33 128 led driver and 39 key scanner
    /// </summary>
    public partial class Ht16k33
    {
        /// <summary>
        /// HT16K33 LED driver and key scan
        /// </summary>
        private readonly II2cPeripheral i2cPeripheral;

        /// <summary>
        /// Display buffer for 16x8 LEDs 
        /// </summary>
        readonly Memory<byte> displayBuffer = new byte[16];

        /// <summary>
        /// Key buffer for 39 keys 
        /// </summary>
        readonly byte[] keyBuffer = new byte[6];

        /// <summary>
        /// Create a new HT16K33 object using the default parameters
        /// </summary>
        /// <param name="address">Address of the bus on the I2C display</param>
        /// <param name="i2cBus">I2C bus instance</param>
        public Ht16k33(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address, 8, 17);

            Initialize();
        }

        void Initialize()
        {
            i2cPeripheral.Write(0x21);

            i2cPeripheral.Write(0x81);

            SetBrightness(Brightness.Maximum);
            ClearDisplay();
        }

        /// <summary>
        /// Set controller to awake / asleep
        /// </summary>
        /// <param name="awake">Awake if true</param>
        public void SetIsAwake(bool awake)
        {
            byte value = (byte)((byte)Register.HT16K33_SS | (byte)(awake ? 1 : 0));

            i2cPeripheral.Write(value);
        }

        /// <summary>
        /// Set display on or off
        /// </summary>
        /// <param name="isOn">On if true</param>
        public void SetDisplayOn(bool isOn)
        {
            byte value = (byte)((byte)Register.HT16K33_DSP | (byte)(isOn ? 1 : 0));

            i2cPeripheral.Write(value);
        }

        /// <summary>
        /// Set display blink rate
        /// </summary>
        /// <param name="blinkRate">The blink rate as a byte</param>
        public void SetBlinkRate(BlinkRate blinkRate)
        {
            byte value = (byte)((byte)Register.HT16K33_DSP | (byte)blinkRate);

            i2cPeripheral.Write(value);
        }

        /// <summary>
        /// Set display brightness
        /// </summary>
        /// <param name="brightness">The brightness</param>
        public void SetBrightness(Brightness brightness)
        {
            byte value = (byte)((byte)Register.HT16K33_DIM | (byte)brightness);

            i2cPeripheral.Write(value);
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        public void ClearDisplay()
        {
            for (int i = 0; i < displayBuffer.Length; i++)
                displayBuffer.Span[i] = 0;

            UpdateDisplay();
        }

        /// <summary>
        /// Refresh the display
        /// </summary>
        public void UpdateDisplay()
        {
            i2cPeripheral.WriteRegister(0x0, displayBuffer.Span);
        }

        /// <summary>
        /// Set an individual led on or off
        /// </summary>
        /// <param name="ledIndex">The led index</param>
        /// <param name="ledOn">True for on</param>
        /// <exception cref="IndexOutOfRangeException">Throws if the index is out of range</exception>
        public void SetLed(byte ledIndex, bool ledOn)
        {
            if (ledIndex > 127)
            {
                throw new IndexOutOfRangeException("LED Index must be between 0 and 127");
            }

            var index = ledIndex / 8;

            if (ledOn)
            {
                displayBuffer.Span[index] = (byte)(displayBuffer.Span[index] | (byte)(1 << (ledIndex % 8)));
            }
            else
            {
                displayBuffer.Span[index] = (byte)(displayBuffer.Span[index] & ~(byte)(1 << (ledIndex % 8)));
            }
        }

        /// <summary>
        /// Toggle an led on or off
        /// </summary>
        /// <param name="ledIndex">The led index</param>
        public void ToggleLed(byte ledIndex)
        {
            var index = ledIndex / 8;

            displayBuffer.Span[index] = (displayBuffer.Span[index] ^= (byte)(1 << ledIndex % 8));
        }

        /// <summary>
        /// Is led at index on
        /// </summary>
        /// <param name="ledIndex">The led index</param>
        /// <returns>True if on</returns>
        public bool IsLedOn(int ledIndex)
        {
            var index = ledIndex / 8;

            return displayBuffer.Span[index] >> ledIndex != 0;
        }
    }
}
