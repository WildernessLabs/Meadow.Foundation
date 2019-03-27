using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;
using System;

namespace SpdtSwitch_Sample
{
    public class SpdtSwitchApp : AppBase<F7Micro, SpdtSwitchApp>
    {
        SpdtSwitch spdtSwitch;

        public SpdtSwitchApp()
        {
            spdtSwitch = new SpdtSwitch(Device.CreateDigitalInputPort(Device.Pins.D02, InterruptMode.EdgeFalling, ResistorMode.PullDown));
            spdtSwitch.Changed += SpdtSwitchChanged;

            Console.WriteLine("Initial switch state, isOn: " + spdtSwitch.IsOn.ToString());
        }

        private void SpdtSwitchChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Switch Changed");
            Console.WriteLine("Switch on: " + spdtSwitch.IsOn.ToString());
        }
    }
}