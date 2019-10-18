using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;

namespace Sensors.Switches.SpdtSwitch_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected SpdtSwitch spdtSwitch;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            spdtSwitch = new SpdtSwitch(Device.CreateDigitalInputPort(Device.Pins.D15, InterruptMode.EdgeBoth, ResistorMode.PullDown));
            spdtSwitch.Changed += (s, e) =>
            {
                Console.WriteLine(spdtSwitch.IsOn ? "Switch is on" : "Switch is off");
            };

            Console.WriteLine("SpdtSwitch ready...");
        }
    }
}