using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Hid;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid
{
    public class WiiNunchuck : WiiExtensionBase
    {
        public IButton CButton { get; } = new WiiExtensionButton();
        public IButton ZButton { get; } = new WiiExtensionButton();

        public IAnalogJoystick AnalogStick { get; } = new WiiExtensionAnalogJoystick(6);


        bool CButtonPressed => (readBuffer[5] >> 1 & 0x01) == 0;
        bool ZButtonPressed => (readBuffer[5] & 0x01) == 0;

        byte JoystickX => readBuffer[0];
        byte JoystickY => readBuffer[1];

        int XAcceleration => (readBuffer[2] << 2) | ((readBuffer[5] >> 2) & 3);
        int YAcceleration => (readBuffer[3] << 2) | ((readBuffer[5] >> 4) & 3);
        int ZAcceleration => (readBuffer[4] << 2) | ((readBuffer[5] >> 6) & 3);

        public WiiNunchuck(II2cBus i2cBus, byte address) : base(i2cBus, address)
        {
        }

        public override void Update()
        {
            base.Update();

            (CButton as WiiExtensionButton).Update(CButtonPressed);
            (ZButton as WiiExtensionButton).Update(ZButtonPressed);

            (AnalogStick as WiiExtensionAnalogJoystick).Update(JoystickX, JoystickY);

          //  Console.WriteLine($"{XAcceleration}, {YAcceleration}, {ZAcceleration}");
        }
    }
}