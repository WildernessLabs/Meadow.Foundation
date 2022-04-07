using System;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    //128 LED driver
    //39 key input
    public partial class Ht16k33
    {
        /// <summary>
        /// HT16K33 LED driver and key scan
        /// </summary>
        private readonly II2cPeripheral i2cPeripheral;

        //display buffer for 16x8 LEDs
        private Memory<byte> displayBuffer = new byte[16];

        //key buffer for 39 keys
        private byte[] keyBuffer = new byte[6];

        /// <summary>
        /// Create a new HT16K33 object using the default parameters
        /// </summary>
        /// <param name="address">Address of the bus on the I2C display.</param>
        /// <param name="i2cBus">I2C bus instance</param>
        public Ht16k33(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            InitHT16K33();
        }

        void InitHT16K33()
        {
            //   SetIsAwake(true);
            //   SetDisplayOn(true);
            //   SetBlinkRate(BlinkRate.Off);

            i2cPeripheral.Write(0x21);

            i2cPeripheral.Write(0x81);

            SetBrightness(Brightness.Maximum);
            ClearDisplay();
        }

        public void SetIsAwake(bool awake)
        {
            byte value = (byte)((byte)Register.HT16K33_SS | (byte)(awake ? 1 : 0));

            i2cPeripheral.Write(value);
        }

        public void SetDisplayOn(bool isOn)
        {
            byte value = (byte)((byte)Register.HT16K33_DSP | (byte)(isOn ? 1 : 0));

            i2cPeripheral.Write(value);
        }

        public void SetBlinkRate(BlinkRate blinkRate)
        {
            byte value = (byte)((byte)Register.HT16K33_DSP | (byte)blinkRate);

            i2cPeripheral.Write(value);
        }

        public void SetBrightness(Brightness brightness)
        {
            byte value = (byte)((byte)Register.HT16K33_DIM | (byte)brightness);

            i2cPeripheral.Write(value);
        }

        public void ClearDisplay()
        {
            for (int i = 0; i < displayBuffer.Length; i++)
                displayBuffer.Span[i] = 0;

            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            i2cPeripheral.WriteRegister(0x0, displayBuffer.Span);
        }

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

        public void ToggleLed(byte ledIndex)
        {
            var index = ledIndex / 8;

            displayBuffer.Span[index] = (displayBuffer.Span[index] ^= (byte)(1 << ledIndex % 8));
        }

        public bool IsLedOn(int ledIndex)
        {
            //need to do some bit math here
            var index = ledIndex / 8;

            //untested
            return displayBuffer.Span[index] >> ledIndex != 0;
        }
    }
}