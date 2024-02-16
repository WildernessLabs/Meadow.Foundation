﻿using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents the IS31FL3731 IC
    /// The Is31fl3731 is a compact LED driver for 144 single LEDs
    /// </summary>
    /// <remarks>Based on https://github.com/adafruit/Adafruit_IS31FL3731 </remarks>
    public partial class Is31fl3731 : II2cPeripheral
    {
        const byte RegConfig = 0x00;
        const byte RegConfigPictureMode = 0x00;

        const byte DisplayOptionRegister = 0x05;
        const byte PictureFrameReg = 0x01;

        const byte RegShutdown = 0x0A;

        const byte CommandRegister = 0xFD;
        const byte CommandFunctionReg = 0x0B;

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
            //Shutdown
            WriteRegister(CommandFunctionReg, RegShutdown, 0x00);
            Thread.Sleep(10);

            //Turn on
            WriteRegister(CommandFunctionReg, RegShutdown, 0x01);
            Thread.Sleep(10);

            WriteRegister(CommandFunctionReg, RegConfig, RegConfigPictureMode);
            Thread.Sleep(10);

            //disable blink mode
            WriteRegister(CommandFunctionReg, DisplayOptionRegister, 0x00);
        }

        /// <summary>
        /// Set the LED state for all LED's in the current Frame
        /// </summary>
        /// <param name="on">true = on, false = off</param>
        public virtual void SetLedState(bool on)
        {
            SetLedState(Frame, on);
        }

        /// <summary>
        /// Sets the state for all LEDs for the specified frame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="on">true = on, false = off</param>
        public virtual void SetLedState(byte frame, bool on)
        {
            if (frame < 0 || frame > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(frame), $"{nameof(frame)} # has to be between 0 and 7");
            }

            for (byte i = 0x00; i <= 0x11; i++)
            {
                if (on)
                {
                    WriteRegister(frame, i, 0xFF);
                }
                else
                {
                    WriteRegister(frame, i, 0x00);
                }
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
        /// Enable/disable blink mode
        /// </summary>
        /// <param name="enabled">true = on, false = off</param>
        /// <param name="period">the blink duration</param>
        public virtual void SetBlinkMode(bool enabled, byte period)
        {
            period = Math.Min(period, (byte)7);

            if (enabled)
            {
                var data = (byte)(period | 0x08);
                WriteRegister(CommandFunctionReg, DisplayOptionRegister, data);
            }
            else
            {
                WriteRegister(CommandFunctionReg, DisplayOptionRegister, 0x00);
            }
        }

        /// <summary>
        /// Turn off all LEDs for the specified frame
        /// </summary>
        public virtual void Clear()
        {
            Clear(Frame);
        }

        /// <summary>
        /// Turn off all LEDs for the specified frame
        /// </summary>
        /// <param name="frame">the frame to clear</param>
        public virtual void Clear(byte frame)
        {
            if (frame < 0 || frame > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(frame), $"{nameof(frame)} # has to be between 0 and 7");
            }

            for (byte i = 0; i <= 144; i++)
            {
                SetLedPwm(frame, i, 0);
            }
        }

        /// <summary>
        /// Turn off all LEDs
        /// </summary>
        public virtual void ClearAllFrames()
        {
            for (byte i = 0; i < 7; i++)
            {
                Clear(i);
            }
        }

        /// <summary>
        /// Set the PWM value for the specified LED
        /// </summary>
        /// <param name="ledIndex">The LED number</param>
        /// <param name="pwm">The pwm value 0-255</param>
        public virtual void SetLedPwm(byte ledIndex, byte pwm)
        {
            SetLedPwm(Frame, ledIndex, pwm);
        }

        /// <summary>
        /// Set the PWM value for the specified LED
        /// </summary>
        /// <param name="frame">Frame number. 0-7</param>
        /// <param name="ledIndex">The LED number. 0-144</param>
        /// <param name="brightness">The pwm value 0-255</param>
        public virtual void SetLedPwm(byte frame, byte ledIndex, byte brightness)
        {
            if (frame < 0 || frame > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(frame), $"{nameof(frame)} # has to be between 0 and 7");
            }

            if (ledIndex < 0 || ledIndex >= 144) { return; }

            WriteRegister(frame, (byte)(0x24 + ledIndex), brightness);
        }

        /// <summary>
        /// Display the specified frame
        /// </summary>
        /// <param name="frame">The frame number. 0-7</param>
        public virtual void DisplayFrame(byte frame)
        {
            frame = Math.Max(frame, (byte)0);
            frame = Math.Min(frame, (byte)7);

            WriteRegister(CommandFunctionReg, PictureFrameReg, frame);
        }

        /// <summary>
        /// Sets the current frame.
        /// </summary>
        /// <param name="frame">The frame number. 0-7</param>
        public virtual void SetFrame(byte frame)
        {
            if (frame < 0 || frame > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(frame), $"{nameof(frame)} must be between 1 and 8");
            }

            Frame = frame;
        }

        /// <summary>
        /// Blink all LEDs for the current frame. Blink Mode muse be enabled
        /// </summary>
        /// <param name="on">true = on, false = off</param>
        public virtual void SetBlinkFunctionOnAllLeds(bool on)
        {
            SetBlinkFunctionOnAllLeds(Frame, on);
        }

        /// <summary>
        /// Blink all the LEDs for the specified frame
        /// </summary>
        /// <param name="frame">frame #</param>
        /// <param name="on">true = on, false = off</param>
        public virtual void SetBlinkFunctionOnAllLeds(byte frame, bool on)
        {
            for (byte j = 0x12; j <= 0x23; j++)
            {
                if (on)
                {
                    WriteRegister(frame, j, 0xFF);
                }
                else
                {
                    WriteRegister(frame, j, 0x00);
                }
            }
        }

        /// <summary>
        /// Select the page/frame
        /// </summary>
        /// <param name="page">page/frame #</param>
        protected virtual void SelectPage(byte page)
        {
            i2cComms.WriteRegister(CommandRegister, page);
        }
    }
}