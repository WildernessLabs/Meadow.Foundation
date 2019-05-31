using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;
using System;

namespace SpdtSwitch_Sample
{
    public class SpdtSwitchApp : App<F7Micro, SpdtSwitchApp>
    {
        SpdtSwitch spdtSwitch;

        public SpdtSwitchApp()
        {
            spdtSwitch = new SpdtSwitch(Device.CreateDigitalInputPort(Device.Pins.D15, InterruptMode.EdgeBoth, ResistorMode.PullDown));
            spdtSwitch.Changed += SpdtSwitchChanged;

            Console.WriteLine("Initial switch state, isOn: " + spdtSwitch.IsOn.ToString());
        }

        private void SpdtSwitchChanged(object sender, EventArgs e)
        {
            Console.WriteLine(spdtSwitch.IsOn?"Switch is on":"Switch is off");
        }
    }
}