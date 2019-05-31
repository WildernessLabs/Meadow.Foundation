using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;
using System;

namespace SpstSwitch_Sample
{
    public class SpstSwitchApp : App<F7Micro, SpstSwitchApp>
    {
        SpstSwitch spstSwitch;

        public SpstSwitchApp()
        {
            spstSwitch = new SpstSwitch(Device.CreateDigitalInputPort(Device.Pins.D02, InterruptMode.EdgeFalling, ResistorMode.PullDown));
            spstSwitch.Changed += SpstSwitchChanged;

            Console.WriteLine("Initial switch state, isOn: " + spstSwitch.IsOn.ToString());
        }

        private void SpstSwitchChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Switch Changed");
            Console.WriteLine("Switch on: " + spstSwitch.IsOn.ToString());
        }
    }
}