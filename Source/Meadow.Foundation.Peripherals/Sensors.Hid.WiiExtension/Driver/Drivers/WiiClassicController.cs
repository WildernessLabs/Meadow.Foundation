using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Hid;

namespace Meadow.Foundation.Sensors.Hid
{
    public class WiiClassicController : WiiExtensionBase
    {
        //D-pad
        public IDigitalJoystick DPad { get; } = new WiiExtensionDPad();

        //A, B, X, Y
        public IButton XButton { get; } = new WiiExtensionButton();
        public IButton YButton { get; } = new WiiExtensionButton();
        public IButton AButton { get; } = new WiiExtensionButton();
        public IButton BButton { get; } = new WiiExtensionButton();

        //L, R, ZL, ZR
        public IButton LButton { get; } = new WiiExtensionButton();
        public IButton RButton { get; } = new WiiExtensionButton();
        public IButton ZLButton { get; } = new WiiExtensionButton();
        public IButton ZRButton { get; } = new WiiExtensionButton();

        //Plus, Minus, Home
        public IButton PlusButton { get; } = new WiiExtensionButton();
        public IButton MinusButton { get; } = new WiiExtensionButton();
        public IButton HomeButton { get; } = new WiiExtensionButton();

        //Analog joysticks
        public IAnalogJoystick LeftAnalogStick { get; } = new WiiExtensionAnalogJoystick(6);

        public IAnalogJoystick RightAnalogStick { get; } = new WiiExtensionAnalogJoystick(5);

        //Analog triggers
        public IAnalogTrigger LeftTrigger { get; } = new WiiExtensionTrigger();
        public IAnalogTrigger RightTrigger { get; } = new WiiExtensionTrigger();


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


        public WiiClassicController(II2cBus i2cBus, byte address) : base(i2cBus, address)
        {
        }

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
            (LeftTrigger as WiiExtensionTrigger).Update(LeftTriggerPosition);
            (RightTrigger as WiiExtensionTrigger).Update(RightTriggerPosition);
        }
    }
}