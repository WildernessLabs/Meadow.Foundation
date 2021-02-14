using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;
using Meadow.Peripherals.Switches;

namespace Sensors.Switches.DipSwitch_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected DipSwitch dipSwitch;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            IDigitalInputPort[] ports =
            {
                Device.CreateDigitalInputPort(Device.Pins.D06, InterruptMode.EdgeRising, ResistorMode.InternalPullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D07, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D08, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D09, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D10, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D11, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D12, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D13, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown),
            };

            dipSwitch = new DipSwitch(ports);
            dipSwitch.Changed += (s,e) =>
            {
                Console.WriteLine("Switch " + e.ItemIndex + " changed to " + (((ISwitch)e.Item).IsOn ? "on" : "off"));
            };

            Console.WriteLine("DipSwitch...");
        }
    }
}