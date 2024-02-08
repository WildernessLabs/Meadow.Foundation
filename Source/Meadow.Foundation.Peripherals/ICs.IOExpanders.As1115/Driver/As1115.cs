using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an As1115 led driver and key scanner
    /// </summary>
    public partial class As1115 : IPixelDisplay, II2cPeripheral, IDisposable
    {
        /// <summary>
        /// Event raised when any key scan button is pressed
        /// </summary>
        public event EventHandler<KeyScanEventArgs> KeyScanPressStarted = default!;

        /// <summary>
        /// Event raised when any key scan button is released
        /// </summary>
        public event EventHandler<KeyScanEventArgs> KeyScanPressEnded = default!;

        /// <summary>
        /// Readonly collection that contains all 16 key scan button objects
        /// </summary>
        public ReadOnlyDictionary<KeyScanButtonType, KeyScanButton>? KeyScanButtons { get; protected set; }

        /// <summary>
        /// Helper method to get IButton object references for keyscan buttons
        /// </summary>
        /// <param name="buttonType">The button type</param>
        /// <returns>The button object reference</returns>
        public IButton GetButton(KeyScanButtonType buttonType) => KeyScanButtons![buttonType];

        /// <summary>
        /// Last button pressed, used internally to raise key up events
        /// </summary>
        private KeyScanButtonType lastButtonPressed = KeyScanButtonType.None;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public int Width => 8;

        /// <inheritdoc/>
        public int Height => 8;

        /// <inheritdoc/>
        public IPixelBuffer PixelBuffer => buffer;

        /// <summary>
        /// The buffer used to store pixel data
        /// </summary>
        private readonly Buffer1bpp buffer = new Buffer1bpp(8, 8);

        /// <summary>
        /// The display decode mode 
        /// BCD character / Hex character / Pixel
        /// </summary>
        public DecodeType DecodeMode { get; private set; }

        /// <summary>
        /// Is the peripheral disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        private readonly IDigitalInterruptPort interruptPort;

        /// <summary>
        /// Create a new AS1115 object using the default parameters for
        /// </summary>
        /// <param name="i2cBus">I2cBus connected to display</param>
        /// <param name="buttonInterruptPin">Interrupt pin</param>
        /// <param name="address">Address of the bus on the I2C display.</param>
        public As1115(II2cBus i2cBus, IPin buttonInterruptPin,
            byte address = (byte)Addresses.Default)
        {
            i2cComms = new I2cCommunications(i2cBus, address);

            interruptPort = buttonInterruptPin.CreateDigitalInterruptPort(
                InterruptMode.EdgeFalling,
                ResistorMode.InternalPullUp);

            interruptPort.Changed += InterruptPort_Changed;

            Initialize();
        }

        private void Initialize()
        {
            var keyDictionary = new Dictionary<KeyScanButtonType, KeyScanButton>();

            //instantiate IButton objects
            for (int i = 0; i < 16; i++)
            {
                keyDictionary.Add((KeyScanButtonType)i, new KeyScanButton());
            }

            KeyScanButtons = new ReadOnlyDictionary<KeyScanButtonType, KeyScanButton>(keyDictionary);

            i2cComms.WriteRegister(REG_SHUTDOWN, REG_SHUTDOWN_RUNNING | REG_SHUTDOWN_RESET_FEATUREREG);

            i2cComms.WriteRegister(REG_SCAN_LIMIT, 0x07);

            SetDecodeMode(DecodeType.Pixel);

            byte[] data = new byte[2];

            //read the key scan registers to clear
            i2cComms.ReadRegister(REG_KEYA, data);
        }

        private void InterruptPort_Changed(object sender, DigitalPortResult e)
        {
            byte[] data = new byte[2];
            i2cComms.ReadRegister(REG_KEYA, data);

            var keyScanButton = GetButtonFromKeyScanRegister(data[0], data[1]);

            if (keyScanButton != KeyScanButtonType.None)
            {
                KeyScanPressStarted?.Invoke(this, new KeyScanEventArgs(keyScanButton, data[0], data[1]));

                KeyScanButtons![keyScanButton].Update(true);

                lastButtonPressed = keyScanButton;
            }
            else
            {
                KeyScanPressEnded?.Invoke(this, new KeyScanEventArgs(lastButtonPressed, data[0], data[1]));

                if (lastButtonPressed != KeyScanButtonType.None)
                {
                    KeyScanButtons![lastButtonPressed].Update(false);
                }
            }
        }

        private KeyScanButtonType GetButtonFromKeyScanRegister(byte keyA, byte keyB)
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
            else if (keyB == 255)
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
            var reg = i2cComms.ReadRegister(REG_FEATURE);

            byte mask = 1 << REG_FEATURE_BLINK;

            if (isEnabled)
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

            i2cComms.WriteRegister(REG_FEATURE, reg);
        }

        /// <summary>
        /// Set the display decode mode
        /// Hexadecimal for matrix leds, or character for 7-segment displays
        /// </summary>
        /// <param name="mode">The decode mode enum</param>
        /// Not currently supported - driver is pixel mode only
        private void SetDecodeMode(DecodeType mode)
        {
            DecodeMode = mode;

            buffer.Clear(true);

            switch (mode)
            {
                case DecodeType.Pixel:
                    i2cComms.WriteRegister(REG_DECODE_MODE, 0);
                    break;
            }
        }

        /// <summary>
        /// Set number to display (left aligned)
        /// </summary>
        /// <param name="value">the number to display</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetNumber(int value)
        {
            //12345678
            if (value > 99999999)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < 8; i++)
            {
                if (DecodeMode == DecodeType.Hexidecimal)
                {
                    SetCharacter((HexCharacterType)(value % 10), i, false);
                }
                else
                {
                    SetCharacter((BcdCharacterType)(value % 10), i, false);
                }

                value /= 10;

                if (value == 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Set a single character
        /// </summary>
        /// <param name="character">the character to display</param>
        /// <param name="digit">the digit index starting from the left</param>
        /// <param name="showDecimal">show the decimal with the character</param>
        public void SetCharacter(BcdCharacterType character, int digit, bool showDecimal = false)
        {
            if (DecodeMode != DecodeType.BCD)
            {
                throw new Exception("SetCharacterBcd requires DecodeMode to be BCD");
            }

            var data = (byte)((byte)character + (showDecimal ? 0x10000000 : 0));

            buffer.Buffer[digit] = data;
        }

        /// <summary>
        /// Set a single character
        /// </summary>
        /// <param name="character">the character to display</param>
        /// <param name="digit">the digit index starting from the left</param>
        /// <param name="showDecimal">show the decimal with the character</param>
        public void SetCharacter(HexCharacterType character, int digit, bool showDecimal = false)
        {
            if (DecodeMode != DecodeType.Hexidecimal)
            {
                throw new Exception("SetCharacterBcd requires DecodeMode to be Hexadecimal");
            }

            var data = (byte)((byte)character + (showDecimal ? 0x10000000 : 0));

            buffer.Buffer[digit] = data;
        }

        /// <summary>
        /// Set display intensity (0-15)
        /// </summary>
        /// <param name="intensity">Intensity from 0-15 (clamps above 15)</param>
        public void SetIntensity(byte intensity)
        {
            intensity = Math.Max(intensity, (byte)15);

            i2cComms.WriteRegister(REG_GLOBAL_INTEN, intensity);
        }

        /// <summary>
        /// Enable/disable test mode
        /// </summary>
        /// <param name="testOn">True to enable, false to disable</param>
        public void TestMode(bool testOn)
        {
            i2cComms.WriteRegister(REG_DECODE_MODE, (byte)(testOn ? 0x01 : 0x00));
        }

        /// <summary>
        /// Update the display
        /// </summary>
        public void Show()
        {
            i2cComms.WriteRegister(REG_DIGIT0, buffer.Buffer[0]);
            i2cComms.WriteRegister(REG_DIGIT1, buffer.Buffer[1]);
            i2cComms.WriteRegister(REG_DIGIT2, buffer.Buffer[2]);
            i2cComms.WriteRegister(REG_DIGIT3, buffer.Buffer[3]);
            i2cComms.WriteRegister(REG_DIGIT4, buffer.Buffer[4]);
            i2cComms.WriteRegister(REG_DIGIT5, buffer.Buffer[5]);
            i2cComms.WriteRegister(REG_DIGIT6, buffer.Buffer[6]);
            i2cComms.WriteRegister(REG_DIGIT7, buffer.Buffer[7]);
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
            if (updateDisplay) { Show(); }
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
        /// <param name="x">X position in pixels</param>
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
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="color">Color to draw - Black will turn pixels off, any color will turn pixels on</param>
        public void DrawPixel(int x, int y, Color color)
        {
            buffer.SetPixel(x, y, color);
        }

        /// <summary>
        /// Draw a pixel at a given location
        /// </summary>
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="enabled">If true, turn led on at location</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            buffer.SetPixel(x, y, enabled);
        }

        /// <summary>
        /// Invert pixel at location (switch on/off)
        /// </summary>
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y position in pixels</param>
        public void InvertPixel(int x, int y)
        {
            buffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Write a pixel buffer to the display buffer
        /// </summary>
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="displayBuffer">Display buffer to write</param>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            buffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
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

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}