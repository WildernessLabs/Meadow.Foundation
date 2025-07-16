using Meadow;
using Meadow.Devices;
using Meadow.Foundation.mikroBUS.Sensors.Hid;
using System;
using System.Threading;

namespace CJoystick_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        CJoystick joystick;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            joystick = new CJoystick(Device.Pins.A02, Device.CreateI2cBus());

            //loop and read digital position 
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Position: {joystick.DigitalPosition}");
                Console.WriteLine($"Pressed: {joystick.State}");

                Thread.Sleep(50);
            }

            //start continous reading
            joystick.StartUpdating(TimeSpan.FromMilliseconds(100));

            //classic events
            joystick.Updated += Joystick_Updated;
            joystick.Clicked += Joystick_Clicked;
        }

        private void Joystick_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Center clicked");
        }

        private void Joystick_Updated(object sender, IChangeResult<Meadow.Peripherals.Sensors.Hid.AnalogJoystickPosition> e)
        {
            Console.WriteLine($"{e.New.Horizontal}, {e.New.Vertical}");
        }

        //<!=SNOP=>
    }
}