using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;

namespace JoyWing_Sample
{
    public class JoyWingApp : App<F7Micro, JoyWingApp>
    {
        JoyWing joyWing;

        public JoyWingApp()
        {
            joyWing = new JoyWing(Device, Device.Pins.D15, Device.Pins.D14, 
                                    Device.Pins.D13, Device.Pins.D12, Device.Pins.D11, 
                                    Device.Pins.D10, Device.Pins.D09);

            joyWing.OnA += (sender, e) => Console.WriteLine("A");
            joyWing.OnB += (sender, e) => Console.WriteLine("B");
            joyWing.OnX += (sender, e) => Console.WriteLine("X");
            joyWing.OnY += (sender, e) => Console.WriteLine("Y");
            joyWing.OnSelect += (sender, e) => Console.WriteLine("Select");
        }
    }
}
