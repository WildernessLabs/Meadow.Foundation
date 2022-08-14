using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class As1115 : IGraphicsDisplay
    {
        public event EventHandler<KeyScanEventArgs> KeyScanPressStarted = null;

        public event EventHandler KeyScanPressEnded = null;

        /// <summary>
        /// As1115 I2C driver
        /// </summary>
        protected II2cPeripheral i2cPeripheral;

        protected IDigitalInputPort interruptPort;

        public ColorType ColorMode => ColorType.Format1bpp;

        /// <summary>
        /// Display width in pixels for 8x8 matrix displays
        /// </summary>
        public int Width => 8;

        /// <summary>
        /// Display height in pixels for 8x8 matrix displays
        /// </summary>
        public int Height => 8;

        /// <summary>
        /// The buffer that holds the pixel data for 8x8 matrix displays
        /// </summary>
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

            interruptPort = device.CreateDigitalInputPort(buttonInterruptPin, 
                InterruptMode.EdgeFalling, 
                ResistorMode.InternalPullUp);

            interruptPort.Changed += InterruptPort_Changed;

            Initialize();
        }

        private void InterruptPort_Changed(object sender, DigitalPortResult e)
        {
            byte[] data = new byte[2];
            i2cPeripheral.ReadRegister(REG_KEYA, data);

            var btn = GetButtonFromKeyScanRegister(data[0], data[1]);

            if (btn != KeyScanButton.None)
            {
                KeyScanPressStarted?.Invoke(this, new KeyScanEventArgs(btn, data[0], data[1]));    
            }
            else
            {
                KeyScanPressEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        KeyScanButton GetButtonFromKeyScanRegister(byte keyA, byte keyB)
        {
            KeyScanButton ret;

            if (keyA == 255)
            {
                ret = keyB switch
                {
                    127 => KeyScanButton.Button9,
                    191 => KeyScanButton.Button10,
                    223 => KeyScanButton.Button11,
                    239 => KeyScanButton.Button12,
                    247 => KeyScanButton.Button13,
                    251 => KeyScanButton.Button14,
                    253 => KeyScanButton.Button15,
                    254 => KeyScanButton.Button16,
                    _ => KeyScanButton.None,
                };
            }
            else if(keyB == 255)
            {
                ret = keyA switch
                {
                    127 => KeyScanButton.Button1,
                    191 => KeyScanButton.Button2,
                    223 => KeyScanButton.Button3,
                    239 => KeyScanButton.Button4,
                    247 => KeyScanButton.Button5,
                    251 => KeyScanButton.Button6,
                    253 => KeyScanButton.Button7,
                    254 => KeyScanButton.Button8,
                    _ => KeyScanButton.None,
                };
            }
            else
            {
                ret = KeyScanButton.None;
            }
            return ret;
        }

        void Initialize()
        {
        //    i2cPeripheral.WriteRegister(REG_SHUTDOWN, REG_SHUTDOWN_RUNNING | REG_SHUTDOWN_PRESERVE_FEATUREREG);

        //    i2cPeripheral.WriteRegister(REG_SELF_ADDR, 0x01);

        //    Thread.Sleep(20);

            i2cPeripheral.WriteRegister(REG_SHUTDOWN, REG_SHUTDOWN_RUNNING | REG_SHUTDOWN_RESET_FEATUREREG);

            i2cPeripheral.WriteRegister(REG_SCAN_LIMIT, 0x07);

            SetDecode(0);

            byte[] data = new byte[2];

            i2cPeripheral.ReadRegister(REG_KEYA, data);
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