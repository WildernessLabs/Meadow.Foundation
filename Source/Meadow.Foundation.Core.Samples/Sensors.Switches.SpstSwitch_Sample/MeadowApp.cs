using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;

namespace Sensors.Switches.SpstSwitch_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected SpstSwitch spstSwitch;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            spstSwitch = new SpstSwitch(Device.CreateDigitalInputPort(Device.Pins.D02, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown));
            spstSwitch.Changed += (s,e) => 
            {
                Console.WriteLine("Switch Changed");
                Console.WriteLine($"Switch on: {spstSwitch.IsOn}");
            };

            Console.WriteLine("SpstSwitch ready...");
        }
    }
}