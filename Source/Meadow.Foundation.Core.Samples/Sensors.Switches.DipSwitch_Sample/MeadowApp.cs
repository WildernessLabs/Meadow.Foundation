using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;
using Meadow.Peripherals.Switches;
using System;
using System.Threading.Tasks;

namespace Sensors.Switches.DipSwitch_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected DipSwitch dipSwitch;

        public override Task Initialize()
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

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}