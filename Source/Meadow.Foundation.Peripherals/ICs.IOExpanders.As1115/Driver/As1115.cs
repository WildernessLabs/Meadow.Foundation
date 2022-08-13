using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class As1115 : IGraphicsDisplay
    {
        /// <summary>
        /// As1115 I2C driver
        /// </summary>
        protected II2cPeripheral i2cPeripheral;

        protected IDigitalInputPort interruptPort;

        public ColorType ColorMode => ColorType.Format1bpp;

        public int Width => 8;

        public int Height => 8;

        public IPixelBuffer PixelBuffer => buffer;

        Buffer1bpp buffer = new Buffer1bpp(8, 8);

        /// <summary>
        /// Create a new AS1115 object using the default parameters for
        /// </summary>
        /// <param name="i2cBus">I2cBus connected to display</param>
        /// <param name="address">Address of the bus on the I2C display.</param>
        public As1115(IMeadowDevice device, II2cBus i2cBus, IPin buttonInterruptPin,
            byte address = (byte)Addresses.Default)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            interruptPort = device.CreateDigitalInputPort(buttonInterruptPin, InterruptMode.EdgeBoth, ResistorMode.ExternalPullDown);

            interruptPort.Changed += InterruptPort_Changed;

            Initialize();
        }

        private void InterruptPort_Changed(object sender, DigitalPortResult e)
        {
            Console.WriteLine("button");
        }

        void Initialize()
        {
        //    i2cPeripheral.WriteRegister(REG_SHUTDOWN, REG_SHUTDOWN_RUNNING | REG_SHUTDOWN_PRESERVE_FEATUREREG);

        //    i2cPeripheral.WriteRegister(REG_SELF_ADDR, 0x01);

        //    Thread.Sleep(20);

            i2cPeripheral.WriteRegister(REG_SHUTDOWN, REG_SHUTDOWN_RUNNING | REG_SHUTDOWN_RESET_FEATUREREG);

            i2cPeripheral.WriteRegister(REG_SCAN_LIMIT, 0x07);

            SetDecode(0);
        }

        public void SetDecode(byte decode)
        {
            i2cPeripheral.WriteRegister(REG_DECODE_MODE, decode);
        }

        //0-15
        public void SetIntensity(byte intensity)
        {
            i2cPeripheral.WriteRegister(REG_GLOBAL_INTEN, intensity);
        }

        public void TestMode(bool testOn)
        {
            i2cPeripheral.WriteRegister(REG_DECODE_MODE, (byte)(testOn?0x01:0x00));
        }

        public void Show()
        {
            i2cPeripheral.WriteRegister(REG_DIGIT0, buffer.Buffer[0]);
            i2cPeripheral.WriteRegister(REG_DIGIT1, buffer.Buffer[1]);
            i2cPeripheral.WriteRegister(REG_DIGIT2, buffer.Buffer[2]);
            i2cPeripheral.WriteRegister(REG_DIGIT3, buffer.Buffer[3]);
            i2cPeripheral.WriteRegister(REG_DIGIT4, buffer.Buffer[4]);
            i2cPeripheral.WriteRegister(REG_DIGIT5, buffer.Buffer[5]);
            i2cPeripheral.WriteRegister(REG_DIGIT6, buffer.Buffer[6]);
            i2cPeripheral.WriteRegister(REG_DIGIT7, buffer.Buffer[7]);
        }

        public void Show(int left, int top, int right, int bottom)
        {
            Show();
        }

        public void Clear(bool updateDisplay = false)
        {
            buffer.Clear();
            if(updateDisplay) { Show(); }
        }

        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            buffer.Fill(fillColor);
            if (updateDisplay) { Show(); }
        }

        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            buffer.Fill(x, y, width, height, fillColor);
        }

        public void DrawPixel(int x, int y, Color color)
        {
            buffer.SetPixel(x, y, color);
        }

        public void DrawPixel(int x, int y, bool colored)
        {
            buffer.SetPixel(x, y, colored);
        }

        public void InvertPixel(int x, int y)
        {
            buffer.InvertPixel(x, y);
        }

        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            buffer.WriteBuffer(x, y, displayBuffer);
        }
    }
}