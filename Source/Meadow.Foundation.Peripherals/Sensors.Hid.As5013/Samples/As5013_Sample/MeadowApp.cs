using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;
using System.Threading.Tasks;

namespace As5013_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        As5013 joystick;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing ...");

            joystick = new As5013(Device.CreateI2cBus());

            joystick.Updated += As5013_Updated;

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            joystick.StartUpdating(TimeSpan.FromMilliseconds(100));

            return Task.CompletedTask;
        }

        private void As5013_Updated(object sender, IChangeResult<Meadow.Peripherals.Sensors.Hid.AnalogJoystickPosition> e)
        {
            Console.WriteLine($"{e.New.Horizontal}, {e.New.Vertical}");
        }

        //<!=SNOP=>
    }
}