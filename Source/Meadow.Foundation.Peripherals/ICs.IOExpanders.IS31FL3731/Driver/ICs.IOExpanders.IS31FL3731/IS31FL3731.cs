using Meadow.Hardware;
using System;
using System.Security.Policy;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents the IS31FL3731 IC.
    /// The Is31fl3731 is a compact LED driver for 144 single LEDs
    /// </summary>
    /// <remarks>Based on https://github.com/adafruit/Adafruit_IS31FL3731 </remarks>
    public class Is31fl3731
    {
        protected const byte RegConfig = 0x00;
        protected const byte RegConfigPictureMode = 0x00;
        protected const byte RegConfigAutoPlayMode = 0x08;
        protected const byte RegConfigAudioPlayMode = 0x18;

        protected const byte ConfPictureMode = 0x00;
        protected const byte ConfAutoFrameMode = 0x04;
        protected const byte ConfAudioMode = 0x08;
        protected const byte DisplayOptionRegister = 0x05;
        protected const byte PictureFrameReg = 0x01;

        protected const byte RegShutdown = 0x0A;
        protected const byte RegAudioSync = 0x06;

        protected const byte CommandRegister = 0xFD;
        protected const byte CommandFunctionReg = 0x0B;  //'page nine'

        protected readonly II2cPeripheral i2cPeripheral;

        public byte Frame { get; private set; }
       
        public Is31fl3731(II2cBus i2cBus, byte address)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);
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
        /// <param name="on">true = on, false = off </param>
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

        protected virtual void WriteRegister(byte frame, byte reg, byte data)
        {
            SelectPage(frame);

            i2cPeripheral.WriteRegister(reg, data);
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
        /// Set the PWM value for the specified LED
        /// </summary>
        /// <param name="lednum">The LED number</param>
        /// <param name="pwm">The pwm value 0-255</param>
        public virtual void SetLedPwm(byte lednum, byte pwm)
        {
            SetLedPwm(Frame, lednum, pwm);
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
            i2cPeripheral.WriteRegister(CommandRegister, page);
        }
    }
}