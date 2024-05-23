using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents the IS31FL3731 IC
    /// The IS31FL3731 is a compact LED driver for 144 single LEDs
    /// </summary>
    /// <remarks>Based on https://github.com/adafruit/Adafruit_IS31FL3731</remarks>
    public partial class Is31fl3731 : II2cPeripheral
    {
        const byte RegConfig = 0x00;
        const byte RegConfigPictureMode = 0x00;

        const byte DisplayOptionRegister = 0x05;
        const byte PictureFrameReg = 0x01;

        const byte RegShutdown = 0x0A;

        const byte CommandRegister = 0xFD;
        const byte CommandFunctionReg = 0x0B;

        const byte MaxFrames = 7;
        const byte MaxLeds = 144;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        /// <summary>
        /// The current frame
        /// </summary>
        public byte Frame { get; private set; }

        /// <summary>
        /// Create a new Is31fl3731 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Is31fl3731(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cComms = new I2cCommunications(i2cBus, address);
            Frame = 0;
        }

        /// <summary>
        /// Initialize the IS31FL3731 by shutting it down, turning it back on and setting it to Picture mode.
        /// </summary>
        public void Initialize()
        {
            // Shutdown
            WriteRegister(CommandFunctionReg, RegShutdown, 0x00);
            Thread.Sleep(10);

            // Turn on
            WriteRegister(CommandFunctionReg, RegShutdown, 0x01);
            Thread.Sleep(10);

            WriteRegister(CommandFunctionReg, RegConfig, RegConfigPictureMode);
            Thread.Sleep(10);

            // Disable blink mode
            WriteRegister(CommandFunctionReg, DisplayOptionRegister, 0x00);
        }

        /// <summary>
        /// Set the LED state for all LEDs in the current Frame
        /// </summary>
        /// <param name="on">true = on, false = off</param>
        public virtual void SetLedState(bool on)
        {
            SetLedState(Frame, on);
        }

        /// <summary>
        /// Sets the state for all LEDs for the specified frame
        /// </summary>
        /// <param name="frame">Frame number</param>
        /// <param name="on">true = on, false = off</param>
        public virtual void SetLedState(byte frame, bool on)
        {
            ValidateFrame(frame);

            for (byte i = 0x00; i <= 0x11; i++)
            {
                WriteRegister(frame, i, on ? (byte)0xFF : (byte)0x00);
            }
        }

        /// <summary>
        /// Write to the current frame register
        /// </summary>
        /// <param name="register">Register to write to</param>
        /// <param name="data">The data value</param>
        protected virtual void WriteRegister(byte register, byte data)
        {
            WriteRegister(Frame, register, data);
        }

        /// <summary>
        /// Write a value to a register
        /// </summary>
        /// <param name="frame">The frame</param>
        /// <param name="register">Register to write to</param>
        /// <param name="data">The data value</param>
        protected virtual void WriteRegister(byte frame, byte register, byte data)
        {
            SelectPage(frame);
            i2cComms.WriteRegister(register, data);
        }

        /// <summary>
        /// Read a value from a register
        /// </summary>
        /// <param name="register">Register to read from</param>
        /// <returns>The data value read from the register</returns>
        protected virtual byte ReadRegister(byte register)
        {
            return i2cComms.ReadRegister(register);
        }

        /// <summary>
        /// Enable/disable blink mode
        /// </summary>
        /// <param name="enabled">true = on, false = off</param>
        /// <param name="period">The blink duration</param>
        public virtual void SetBlinkMode(bool enabled, byte period)
        {
            period = Math.Min(period, (byte)7);

            WriteRegister(CommandFunctionReg, DisplayOptionRegister, enabled ? (byte)(period | 0x08) : (byte)0x00);
        }

        /// <summary>
        /// Turn off all LEDs for the current frame
        /// </summary>
        public virtual void Clear()
        {
            Clear(Frame);
        }

        /// <summary>
        /// Turn off all LEDs for the specified frame
        /// </summary>
        /// <param name="frame">The frame to clear</param>
        public virtual void Clear(byte frame)
        {
            ValidateFrame(frame);

            for (byte i = 0; i < MaxLeds; i++)
            {
                SetLedPwm(frame, i, 0);
            }
        }

        /// <summary>
        /// Turn off all LEDs
        /// </summary>
        public virtual void ClearAllFrames()
        {
            for (byte i = 0; i <= MaxFrames; i++)
            {
                Clear(i);
            }
        }

        /// <summary>
        /// Set the PWM value for the specified LED
        /// </summary>
        /// <param name="ledIndex">The LED number</param>
        /// <param name="pwm">The PWM value (0-255)</param>
        public virtual void SetLedPwm(byte ledIndex, byte pwm)
        {
            SetLedPwm(Frame, ledIndex, pwm);
        }

        /// <summary>
        /// Set the PWM value for the specified LED
        /// </summary>
        /// <param name="frame">Frame number (0-7)</param>
        /// <param name="ledIndex">The LED number (0-144)</param>
        /// <param name="brightness">The PWM value (0-255)</param>
        public virtual void SetLedPwm(byte frame, byte ledIndex, byte brightness)
        {
            ValidateFrame(frame);
            if (ledIndex >= MaxLeds) return;

            WriteRegister(frame, (byte)(0x24 + ledIndex), brightness);
        }

        /// <summary>
        /// Get the PWM value for the specified LED
        /// </summary>
        /// <param name="frame">Frame number (0-7)</param>
        /// <param name="ledIndex">The LED number (0-144)</param>
        /// <returns>The PWM value 0-255</returns>
        public virtual byte GetLedPwm(byte frame, byte ledIndex)
        {
            ValidateFrame(frame);
            if (ledIndex >= MaxLeds)
            {
                throw new ArgumentOutOfRangeException(nameof(ledIndex), $"LED index must be between 0 and {MaxLeds - 1}");
            }

            SelectPage(frame);
            return ReadRegister((byte)(0x24 + ledIndex));
        }

        /// <summary>
        /// Display the specified frame
        /// </summary>
        /// <param name="frame">The frame number (0-7)</param>
        public virtual void DisplayFrame(byte frame)
        {
            frame = Math.Clamp(frame, (byte)0, MaxFrames);

            WriteRegister(CommandFunctionReg, PictureFrameReg, frame);
        }

        /// <summary>
        /// Sets the current frame.
        /// </summary>
        /// <param name="frame">The frame number (0-7)</param>
        public virtual void SetFrame(byte frame)
        {
            ValidateFrame(frame);
            Frame = frame;
        }

        /// <summary>
        /// Blink all LEDs for the current frame - blink Mode must be enabled
        /// </summary>
        /// <param name="on">true = on, false = off</param>
        public virtual void SetBlinkFunctionOnAllLeds(bool on)
        {
            SetBlinkFunctionOnAllLeds(Frame, on);
        }

        /// <summary>
        /// Blink all the LEDs for the specified frame
        /// </summary>
        /// <param name="frame">Frame number</param>
        /// <param name="on">true = on, false = off</param>
        public virtual void SetBlinkFunctionOnAllLeds(byte frame, bool on)
        {
            for (byte j = 0x12; j <= 0x23; j++)
            {
                WriteRegister(frame, j, on ? (byte)0xFF : (byte)0x00);
            }
        }

        /// <summary>
        /// Select the page/frame
        /// </summary>
        /// <param name="page">Page/frame number</param>
        protected virtual void SelectPage(byte page)
        {
            i2cComms.WriteRegister(CommandRegister, page);
        }

        private void ValidateFrame(byte frame)
        {
            if (frame < 0 || frame > MaxFrames)
            {
                throw new ArgumentOutOfRangeException(nameof(frame), $"{nameof(frame)} must be between 0 and {MaxFrames}");
            }
        }
    }
}
