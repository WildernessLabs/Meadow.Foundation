using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;

namespace As5013_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            var joystick = new As5013(Device.CreateI2cBus());

            joystick.StartUpdating(TimeSpan.FromMilliseconds(100));

            joystick.Updated += As5013_Updated;
        }

        private void As5013_Updated(object sender, IChangeResult<Meadow.Peripherals.Sensors.Hid.AnalogJoystickPosition> e)
        {
            Console.WriteLine($"{e.New.Horizontal}, {e.New.Vertical}");
        }

        //<!=SNOP=>
    }
}