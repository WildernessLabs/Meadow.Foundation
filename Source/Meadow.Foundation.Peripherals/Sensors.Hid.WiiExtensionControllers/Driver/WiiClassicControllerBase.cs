﻿using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Abstract base class that represents 
    /// Nintendo Wiimote I2C extension controllers 
    /// Based on the Wii classic controller including:
    /// Classic Pro, Snes classic, and Nes classic controllers
    /// </summary>
    public abstract class WiiClassicControllerBase : WiiExtensionControllerBase
    {
        /// <summary>
        /// Get the x-axis value for the left analog joystick (classic and classic pro)
        /// 0-63 in normal mode, 0-255 is high resolution mode
        /// </summary>
        protected byte LeftJoystickX => (byte)(useHighResolutionMode ? ReadBuffer[0] : (ReadBuffer[0] & 0x3F));
        /// <summary>
        /// Get the y-axis value for the left analog joystick (classic and classic pro)
        /// 0-63 in normal mode, 0-255 is high resolution mode
        /// </summary>
        protected byte LeftJoystickY => (byte)(useHighResolutionMode ? ReadBuffer[1] : (ReadBuffer[1] & 0x3F));

        /// <summary>
        /// Get the x-axis value for the right analog joystick (classic and classic pro)
        /// 0-31 in normal mode, 0-255 is high resolution mode
        /// </summary>
        protected byte RightJoystickX => (byte)(useHighResolutionMode ? ReadBuffer[2] :
            ((byte)((ReadBuffer[0] >> 3) & 0x18) | (byte)((ReadBuffer[1] >> 5) & 0x06) | ReadBuffer[2] >> 7 & 0x01));
        /// <summary>
        /// Get the y-axis value for the right analog joystick (classic and classic pro)
        /// 0-31 in normal mode, 0-255 is high resolution mode
        /// </summary>
        protected byte RightJoystickY => (byte)(useHighResolutionMode ? ReadBuffer[3] : ((ReadBuffer[2]) & 0x1F));

        /// <summary>
        /// Left trigger position (original classic controller only)
        /// 0-31 in normal mode, 0-255 is high resolution mode
        /// </summary>
        protected byte LeftTriggerPosition => (byte)(useHighResolutionMode ? ReadBuffer[4] :
            (byte)((ReadBuffer[2] >> 2) & 0x18) | (byte)((ReadBuffer[3] >> 5) & 0x07));
        /// <summary>
        /// Right trigger position (original classic controller only)
        /// 0-31 in normal mode, 0-255 is high resolution mode
        /// </summary>
        protected byte RightTriggerPosition => (byte)(useHighResolutionMode ? ReadBuffer[5] : ((ReadBuffer[3]) & 0x1F));

        /// <summary>
        /// Is Plus (Start) button pressed
        /// </summary>
        protected bool PlusButtonPressed => (ReadBuffer[useHighResolutionMode ? 6 : 4] >> 2 & 0x01) == 0;
        /// <summary>
        /// Is Minus (Select) button pressed
        /// </summary>
        protected bool MinusButtonPressed => (ReadBuffer[useHighResolutionMode ? 6 : 4] >> 4 & 0x01) == 0;
        /// <summary>
        /// Is Home button pressed
        /// </summary>
        protected bool HomeButtonPressed => (ReadBuffer[useHighResolutionMode ? 6 : 4] >> 3 & 0x01) == 0;

        /// <summary>
        /// Is L button pressed
        /// </summary>
        protected bool LButtonPressed => (ReadBuffer[useHighResolutionMode ? 6 : 4] >> 5 & 0x01) == 0;
        /// <summary>
        /// Is R button pressed
        /// </summary>
        protected bool RButtonPressed => (ReadBuffer[useHighResolutionMode ? 6 : 4] >> 1 & 0x01) == 0;
        /// <summary>
        /// Is ZL button pressed
        /// </summary>
        protected bool ZLButtonPressed => (ReadBuffer[useHighResolutionMode ? 7 : 5] >> 7 & 0x01) == 0;
        /// <summary>
        /// Is ZR button pressed
        /// </summary>
        protected bool ZRButtonPressed => (ReadBuffer[useHighResolutionMode ? 7 : 5] >> 2 & 0x01) == 0;

        /// <summary>
        /// Is X button pressed
        /// </summary>
        protected bool XButtonPressed => (ReadBuffer[useHighResolutionMode ? 7 : 5] >> 3 & 0x01) == 0;
        /// <summary>
        /// Is Y button pressed
        /// </summary>
        protected bool YButtonPressed => (ReadBuffer[useHighResolutionMode ? 7 : 5] >> 5 & 0x01) == 0;
        /// <summary>
        /// Is A button pressed
        /// </summary>
        protected bool AButtonPressed => (ReadBuffer[useHighResolutionMode ? 7 : 5] >> 4 & 0x01) == 0;
        /// <summary>
        /// Is B button pressed
        /// </summary>
        protected bool BButtonPressed => (ReadBuffer[useHighResolutionMode ? 7 : 5] >> 6 & 0x01) == 0;

        /// <summary>
        /// Is D-pad left pressed
        /// </summary>
        protected bool DPadLeftPressed => (ReadBuffer[useHighResolutionMode ? 7 : 5] >> 1 & 0x01) == 0;
        /// <summary>
        /// Is D-pad right pressed
        /// </summary>
        protected bool DPadRightPressed => (ReadBuffer[useHighResolutionMode ? 6 : 4] >> 7 & 0x01) == 0;
        /// <summary>
        /// Is D-pad up pressed
        /// </summary>
        protected bool DPadUpPressed => (ReadBuffer[useHighResolutionMode ? 7 : 5] >> 0 & 0x01) == 0;
        /// <summary>
        /// Is D-pad down pressed
        /// </summary>
        protected bool DPadDownPressed => (ReadBuffer[useHighResolutionMode ? 6 : 4] >> 6 & 0x01) == 0;

        /// <summary>
        /// Use high resolution mode for analog controls (Classic and Classic Pro controllers)
        /// </summary>
        protected bool useHighResolutionMode;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="i2cBus">the I2C bus connected to the extension controller</param>
        /// <param name="address">The extension controller address</param>
        protected WiiClassicControllerBase(II2cBus i2cBus, byte address) : base(i2cBus, address)
        {
        }

        /// <summary>
        /// Initialize the extension controller
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            if (useHighResolutionMode)
            {
                i2cComms.WriteRegister(0xFE, 0x03);
            }
            else
            {
                i2cComms.WriteRegister(0xFE, 0x00);
            }
        }
    }
}