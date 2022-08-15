using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class As1115 : IGraphicsDisplay, IDisposable
    {
        /// <summary>
        /// Event raised when any key scan button is pressed
        /// </summary>
        public event EventHandler<KeyScanEventArgs> KeyScanPressStarted = null;

        /// <summary>
        /// Event raised when any key scan button is released
        /// </summary>
        public event EventHandler<KeyScanEventArgs> KeyScanPressEnded = null;

        /// <summary>
        /// Readonly collection that contains all 16 key scan button objects
        /// </summary>
        public ReadOnlyDictionary<KeyScanButtonType, KeyScanButton> KeyScanButtons { get; protected set; }

        /// <summary>
        /// Helper method to get IButton object references for keyscan buttons
        /// </summary>
        /// <param name="buttonType">The button type</param>
        /// <returns>The button object reference</returns>
        public IButton GetButton(KeyScanButtonType buttonType) => KeyScanButtons[buttonType];

        /// <summary>
        /// Last button pressed, used internally to raise key up events
        /// </summary>
        KeyScanButtonType lastButtonPressed = KeyScanButtonType.None;

        /// <summary>
        /// As1115 I2C driver
        /// </summary>
        protected II2cPeripheral i2cPeripheral;

        /// <summary>
        /// The display color mode (1 bit per pixel)
        /// </summary>
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

        /// <summary>
        /// The buffer used to store pixel data
        /// </summary>
        readonly Buffer1bpp buffer = new Buffer1bpp(8, 8);

        /// <summary>
        /// Is the peripheral disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        readonly IDigitalInputPort interruptPort;

        /// <summary>
        /// Create a new AS1115 object using the default parameters for
        /// </summary>
        /// <param name="device">Meadow device object</param>
        /// <param name="i2cBus">I2cBus connected to display</param>
        /// <param name="buttonInterruptPin">Interrupt pin</param>
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

        void Initialize()
        {
            var keyDictionary = new Dictionary<KeyScanButtonType, KeyScanButton>();

            //instantiate IButton objects
            for (int i = 0; i < 16; i++)
            {
                keyDictionary.Add((KeyScanButtonType)i, new KeyScanButton());
            }

            KeyScanButtons = new ReadOnlyDictionary<KeyScanButtonType, KeyScanButton>(keyDictionary);

            i2cPeripheral.WriteRegister(REG_SHUTDOWN, REG_SHUTDOWN_RUNNING | REG_SHUTDOWN_RESET_FEATUREREG);

            i2cPeripheral.WriteRegister(REG_SCAN_LIMIT, 0x07);

            SetDecodeMode(DecodeMode.Pixel);

            byte[] data = new byte[2];

            //read the key scan registers to clear
            i2cPeripheral.ReadRegister(REG_KEYA, data);
        }

        private void InterruptPort_Changed(object sender, DigitalPortResult e)
        {
            byte[] data = new byte[2];
            i2cPeripheral.ReadRegister(REG_KEYA, data);

            var keyScanButton = GetButtonFromKeyScanRegister(data[0], data[1]);

            if (keyScanButton != KeyScanButtonType.None)
            {
                KeyScanPressStarted?.Invoke(this, new KeyScanEventArgs(keyScanButton, data[0], data[1]));

                KeyScanButtons[keyScanButton].Update(true);

                lastButtonPressed = keyScanButton;
            }
            else
            {
                KeyScanPressEnded?.Invoke(this, new KeyScanEventArgs(lastButtonPressed, data[0], data[1]));

                if(lastButtonPressed != KeyScanButtonType.None)
                {
                    KeyScanButtons[lastButtonPressed].Update(false);
                }
            }
        }

        KeyScanButtonType GetButtonFromKeyScanRegister(byte keyA, byte keyB)
        {
            KeyScanButtonType ret;

            if (keyA == 255)
            {
                ret = keyB switch
                {
                    127 => KeyScanButtonType.Button9,
                    191 => KeyScanButtonType.Button10,
                    223 => KeyScanButtonType.Button11,
                    239 => KeyScanButtonType.Button12,
                    247 => KeyScanButtonType.Button13,
                    251 => KeyScanButtonType.Button14,
                    253 => KeyScanButtonType.Button15,
                    254 => KeyScanButtonType.Button16,
                    _ => KeyScanButtonType.None,
                };
            }
            else if(keyB == 255)
            {
                ret = keyA switch
                {
                    127 => KeyScanButtonType.Button1,
                    191 => KeyScanButtonType.Button2,
                    223 => KeyScanButtonType.Button3,
                    239 => KeyScanButtonType.Button4,
                    247 => KeyScanButtonType.Button5,
                    251 => KeyScanButtonType.Button6,
                    253 => KeyScanButtonType.Button7,
                    254 => KeyScanButtonType.Button8,
                    _ => KeyScanButtonType.None,
                };
            }
            else
            {
                ret = KeyScanButtonType.None;
            }
            return ret;
        }

        /// <summary>
        /// Enable or disable display blinking
        /// </summary>
        /// <param name="isEnabled">Display will blink if true</param>
        /// <param name="fastBlink">True for fast blink (period of 1s), False for slow blink (period of 2s)</param>
        public void EnableBlink(bool isEnabled, bool fastBlink = true)
        {
            var reg = i2cPeripheral.ReadRegister(REG_FEATURE);

            byte mask = 1 << REG_FEATURE_BLINK;

            if(isEnabled)
            {
                reg |= mask;
            }
            else
            {
                reg &= (byte)~mask;
            }

            mask = 1 << REG_FEATURE_BLINKFREQ;
            if (!fastBlink)
            {
                reg |= mask;
            }
            else
            {
                reg &= (byte)~mask;
            }

            i2cPeripheral.WriteRegister(REG_FEATURE, reg);
        }

        /// <summary>
        /// Set the display decode mode
        /// Hexidecimal for matrix leds, or character for 7-segment displays
        /// </summary>
        /// <param name="mode">The decode mode enum</param>
        /// Not currently supported - driver is pixel mode only
        void SetDecodeMode(DecodeMode mode)
        {
            buffer.Clear(true);

            switch (mode)
            {
                case DecodeMode.Pixel:
                    i2cPeripheral.WriteRegister(REG_DECODE_MODE, 0);
                    break;
            }
        }

        /// <summary>
        /// Set display intensity (0-15)
        /// </summary>
        /// <param name="intensity">Instensity from 0-15 (clamps above 15)</param>
        public void SetIntensity(byte intensity)
        {
            intensity = Math.Max(intensity, (byte)15);

            i2cPeripheral.WriteRegister(REG_GLOBAL_INTEN, intensity);
        }

        /// <summary>
        /// Enable/disable test mode
        /// </summary>
        /// <param name="testOn">True to enable, false to disable</param>
        public void TestMode(bool testOn)
        {
            i2cPeripheral.WriteRegister(REG_DECODE_MODE, (byte)(testOn?0x01:0x00));
        }

        /// <summary>
        /// Update the display
        /// </summary>
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

        /// <summary>
        /// Update the display from the display buffer
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        public void Show(int left, int top, int right, int bottom)
        {
            Show();
        }

        /// <summary>
        /// Clear the display buffer
        /// </summary>
        /// <param name="updateDisplay">If true, update the display</param>
        public void Clear(bool updateDisplay = false)
        {
            buffer.Clear();
            if(updateDisplay) { Show(); }
        }

        /// <summary>
        /// Fill the display buffer with a color
        /// Black will clear the display, any other color will turn on all leds
        /// </summary>
        /// <param name="fillColor">The color to fill</param>
        /// <param name="updateDisplay">Update the display</param>
        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            buffer.Fill(fillColor);
            if (updateDisplay) { Show(); }
        }

        /// <summary>
        /// Fill a region of the display buffer with a color
        /// Black will clear the display, any other color will turn on all leds
        /// </summary>
        /// <param name="x">X postion in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="fillColor">Color to fill - Black will turn pixels off, any color will turn pixels on</param>
        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            buffer.Fill(x, y, width, height, fillColor);
        }

        /// <summary>
        /// Draw a pixel at a given location
        /// </summary>
        /// <param name="x">X postion in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="color">Color to draw - Black will turn pixels off, any color will turn pixels on</param>
        public void DrawPixel(int x, int y, Color color)
        {
            buffer.SetPixel(x, y, color);
        }

        /// <summary>
        /// Draw a pixel at a given location
        /// </summary>
        /// <param name="x">X postion in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="colored">If true, turn led on at location</param>
        public void DrawPixel(int x, int y, bool colored)
        {
            buffer.SetPixel(x, y, colored);
        }

        /// <summary>
        /// Invert pixel at location (switch on/off)
        /// </summary>
        /// <param name="x">X postion in pixels</param>
        /// <param name="y">Y position in pixels</param>
        public void InvertPixel(int x, int y)
        {
            buffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Write a pixel buffer to the display buffer
        /// </summary>
        /// <param name="x">X postion in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="displayBuffer">Display buffer to write</param>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            buffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    interruptPort.Dispose();
                }
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose Peripheral
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}