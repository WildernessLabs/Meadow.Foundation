using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;
using Meadow.Peripherals.Switches;

namespace Sensors.Switches.DipSwitch_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        protected DipSwitch dipSwitch;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            IDigitalInputPort[] ports =
            {
                Device.CreateDigitalInputPort(Device.Pins.D06, InterruptMode.EdgeRising, ResistorMode.InternalPullDown),
            };

            dipSwitch = new DipSwitch(ports);
            dipSwitch.Changed += (s,e) =>
            {
                Console.WriteLine("Switch " + e.ItemIndex + " changed to " + (((ISwitch)e.Item).IsOn ? "on" : "off"));
            };

            Console.WriteLine("DipSwitch...");
        }

        //<!=SNOP=>
    }
}