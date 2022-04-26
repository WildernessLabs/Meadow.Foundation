using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Hid;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents a Nintendo Wii I2C Classic controller
    /// </summary>
    public class WiiClassicController : WiiExtensionBase
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
        /// Left analog jostick (6 bits of precision)
        /// </summary>
        public IAnalogJoystick LeftAnalogStick { get; } = new WiiExtensionAnalogJoystick(6);

        /// <summary>
        /// Right analog jostick (5 bits of precision)
        /// </summary>
        public IAnalogJoystick RightAnalogStick { get; } = new WiiExtensionAnalogJoystick(5);

        /// <summary>
        /// Left analog trigger (5 bits of precision)
        /// </summary>
        public IAnalogTrigger LeftTrigger { get; } = new WiiExtensionAnalogTrigger();
        /// <summary>
        /// Right analog trigger (5 bits of precision)
        /// </summary>
        public IAnalogTrigger RightTrigger { get; } = new WiiExtensionAnalogTrigger();

        //helper internal properties
        byte LeftJoystickX => (byte)(readBuffer[0] & 0x3F);
        byte LeftJoystickY => (byte)(readBuffer[1] & 0x3F);

        byte RightJoystickX => (byte)((byte)((readBuffer[0] >> 3) & 0x18) | (byte)((readBuffer[1] >> 5) & 0x06) | readBuffer[2] >> 7 & 0x01);
        byte RightJoystickY => (byte)((readBuffer[2]) & 0x1F);

        byte LeftTriggerPosition => (byte)((byte)((readBuffer[2] >> 2) & 0x18) | (byte)((readBuffer[3] >> 5) & 0x07));
        byte RightTriggerPosition => (byte)((readBuffer[3]) & 0x1F);

        bool PlusButtonPressed => (readBuffer[4] >> 2 & 0x01) == 0;
        bool MinusButtonPressed => (readBuffer[4] >> 4 & 0x01) == 0;
        bool HomeButtonPressed => (readBuffer[4] >> 3 & 0x01) == 0;

        bool LButtonPressed => (readBuffer[4] >> 5 & 0x01) == 0;
        bool RButtonPressed => (readBuffer[4] >> 1 & 0x01) == 0;
        bool ZLButtonPressed => (readBuffer[5] >> 7 & 0x01) == 0;
        bool ZRButtonPressed => (readBuffer[5] >> 2 & 0x01) == 0;


        bool DPadLeftPressed => (readBuffer[5] >> 1 & 0x01) == 0;
        bool DPadRightPressed => (readBuffer[4] >> 7 & 0x01) == 0;
        bool DPadUpPressed => (readBuffer[5] >> 0 & 0x01) == 0;
        bool DPadDownPressed => (readBuffer[4] >> 6 & 0x01) == 0;

        bool XButtonPressed => (readBuffer[5] >> 3 & 0x01) == 0;
        bool YButtonPressed => (readBuffer[5] >> 5 & 0x01) == 0;
        bool AButtonPressed => (readBuffer[5] >> 4 & 0x01) == 0;
        bool BButtonPressed => (readBuffer[5] >> 6 & 0x01) == 0;

        /// <summary>
        /// Creates a Wii Classic Controller object
        /// </summary>
        /// <param name="i2cBus">the I2C bus connected to controller</param>
        /// <param name="address">the address of the controller</param>
        public WiiClassicController(II2cBus i2cBus, byte address) : base(i2cBus, address)
        {
        }

        /// <summary>
        /// Get the latest sensor data from the device
        /// </summary>
        public override void Update()
        {
            base.Update();

            //DPad
            (DPad as WiiExtensionDPad).Update(DPadLeftPressed, DPadRightPressed, DPadUpPressed, DPadDownPressed);

            //Analog sticks
            (LeftAnalogStick as WiiExtensionAnalogJoystick).Update(LeftJoystickX, LeftJoystickY);
            (RightAnalogStick as WiiExtensionAnalogJoystick).Update(RightJoystickX, RightJoystickY);

            //A, B, X, Y
            (XButton as WiiExtensionButton).Update(XButtonPressed);
            (YButton as WiiExtensionButton).Update(YButtonPressed);
            (AButton as WiiExtensionButton).Update(AButtonPressed);
            (BButton as WiiExtensionButton).Update(BButtonPressed);

            //+, -, home
            (PlusButton as WiiExtensionButton).Update(PlusButtonPressed);
            (MinusButton as WiiExtensionButton).Update(MinusButtonPressed);
            (HomeButton as WiiExtensionButton).Update(HomeButtonPressed);

            //L, R, ZL, ZR
            (LButton as WiiExtensionButton).Update(LButtonPressed);
            (RButton as WiiExtensionButton).Update(RButtonPressed);
            (ZLButton as WiiExtensionButton).Update(ZLButtonPressed);
            (ZRButton as WiiExtensionButton).Update(ZRButtonPressed);

            //analog triggers
            (LeftTrigger as WiiExtensionAnalogTrigger).Update(LeftTriggerPosition);
            (RightTrigger as WiiExtensionAnalogTrigger).Update(RightTriggerPosition);
        }
    }
}