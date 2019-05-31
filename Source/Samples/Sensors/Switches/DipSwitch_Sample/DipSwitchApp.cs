﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;
using Meadow.Peripherals.Switches;
using System;

namespace DipSwitch_Sample
{
    public class DipSwitchApp : App<F7Micro, DipSwitchApp>
    {
        DipSwitch dipSwitch;

        public DipSwitchApp()
        {
            IDigitalInputPort[] ports =
            {
                Device.CreateDigitalInputPort(Device.Pins.D06, InterruptMode.LevelHigh, ResistorMode.PullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D07, InterruptMode.EdgeFalling, ResistorMode.PullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D08, InterruptMode.EdgeFalling, ResistorMode.PullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D09, InterruptMode.EdgeFalling, ResistorMode.PullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D10, InterruptMode.EdgeFalling, ResistorMode.PullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D11, InterruptMode.EdgeFalling, ResistorMode.PullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D12, InterruptMode.EdgeFalling, ResistorMode.PullDown),
         //       Device.CreateDigitalInputPort(Device.Pins.D13, InterruptMode.EdgeFalling, ResistorMode.PullDown),
            };

            dipSwitch = new DipSwitch(ports);

            dipSwitch.Changed += DipSwitchChanged;
        }

        private void DipSwitchChanged(object sender, Meadow.Foundation.ArrayEventArgs e)
        {
            Console.WriteLine("Switch " + e.ItemIndex + " changed to " + (((ISwitch)e.Item).IsOn ? "on" : "off"));
        }
    }
}