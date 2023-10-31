using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Hid;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents a Nintendo Wii I2C Classic Controller Pro
    /// </summary>
    public class WiiClassicControllerPro : WiiClassicControllerBase
    {
        /// <summary>
        /// D-pad
        /// </summary>
        public IDigitalJoystick DPad { get; } = new WiiExtensionDPad();

        /// <summary>
        /// X Button
        /// </summary>
        public IButton XButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// Y Button
        /// </summary>
        public IButton YButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// A Button
        /// </summary>
        public IButton AButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// B Button
        /// </summary>
        public IButton BButton { get; } = new WiiExtensionButton();

        /// <summary>
        /// L Button
        /// </summary>
        public IButton LButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// R Button
        /// </summary>
        public IButton RButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// ZL Button (at bottom of trigger)
        /// </summary>
        public IButton ZLButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// ZR Button (at bottom of trigger)
        /// </summary>
        public IButton ZRButton { get; } = new WiiExtensionButton();

        /// <summary>
        /// + Button
        /// </summary>
        public IButton PlusButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// - Button
        /// </summary>
        public IButton MinusButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// Home Button
        /// </summary>
        public IButton HomeButton { get; } = new WiiExtensionButton();

        /// <summary>
        /// Left analog joystick (6 or 8 bits of precision)
        /// </summary>
        public IAnalogJoystick LeftAnalogStick { get; }

        /// <summary>
        /// Right analog joystick (5 or 8 bits of precision)
        /// </summary>
        public IAnalogJoystick RightAnalogStick { get; }

        /// <summary>
        /// Creates a Wii Classic Controller object
        /// </summary>
        /// <param name="i2cBus">the I2C bus connected to controller</param>
        /// <param name="useHighResolutionMode">Enable high resolution mode analog sticks and triggers (8 bits of precision)</param>
        public WiiClassicControllerPro(II2cBus i2cBus, bool useHighResolutionMode = false) : base(i2cBus, (byte)Addresses.Default)
        {
            this.useHighResolutionMode = useHighResolutionMode;

            LeftAnalogStick = new WiiExtensionAnalogJoystick((byte)(useHighResolutionMode ? 8 : 6));
            RightAnalogStick = new WiiExtensionAnalogJoystick((byte)(useHighResolutionMode ? 8 : 5));
        }

        /// <summary>
        /// Get the latest sensor data from the device
        /// </summary>
        public override void Update()
        {
            if (useHighResolutionMode)
            {
                i2cComms.WriteRegister(0, 0);
                i2cComms.Read(ReadBuffer[..8]);
            }
            else
            {
                base.Update();
            }

            //DPad
            (DPad as WiiExtensionDPad)!.Update(DPadLeftPressed, DPadRightPressed, DPadUpPressed, DPadDownPressed);

            //Analog sticks
            (LeftAnalogStick as WiiExtensionAnalogJoystick)!.Update(LeftJoystickX, LeftJoystickY);
            (RightAnalogStick as WiiExtensionAnalogJoystick)!.Update(RightJoystickX, RightJoystickY);

            //A, B, X, Y
            (XButton as WiiExtensionButton)!.Update(XButtonPressed);
            (YButton as WiiExtensionButton)!.Update(YButtonPressed);
            (AButton as WiiExtensionButton)!.Update(AButtonPressed);
            (BButton as WiiExtensionButton)!.Update(BButtonPressed);

            //+, -, home
            (PlusButton as WiiExtensionButton)!.Update(PlusButtonPressed);
            (MinusButton as WiiExtensionButton)!.Update(MinusButtonPressed);
            (HomeButton as WiiExtensionButton)!.Update(HomeButtonPressed);

            //L, R, ZL, ZR
            (LButton as WiiExtensionButton)!.Update(LButtonPressed);
            (RButton as WiiExtensionButton)!.Update(RButtonPressed);
            (ZLButton as WiiExtensionButton)!.Update(ZLButtonPressed);
            (ZRButton as WiiExtensionButton)!.Update(ZRButtonPressed);
        }
    }
}