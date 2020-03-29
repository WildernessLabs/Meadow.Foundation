using System;
using System.Collections.Generic;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Buttons;

namespace Sensors.Buttons.PushButton_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        List<PushButton> pushButtons;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            pushButtons = new List<PushButton> 
            {
                new PushButton(Device, Device.Pins.D00), // <- Not working (D00 - D04) (D00 - D07) (*)
                new PushButton(Device, Device.Pins.D01), 
                new PushButton(Device, Device.Pins.D02), // <- (*)
                new PushButton(Device, Device.Pins.D03),
                new PushButton(Device, Device.Pins.D04), // <- (*)
                new PushButton(Device, Device.Pins.D05), // <- Not working (D04 - D07) (D00 - D07) (*)
                new PushButton(Device, Device.Pins.D06),
                new PushButton(Device, Device.Pins.D07),
                new PushButton(Device, Device.Pins.D08),
                new PushButton(Device, Device.Pins.D09),
                new PushButton(Device, Device.Pins.D10),
                new PushButton(Device, Device.Pins.D11),
                new PushButton(Device, Device.Pins.D12),
                new PushButton(Device, Device.Pins.D13),
                new PushButton(Device, Device.Pins.D14), // <- Not working (D12 - D15) (D08 - D15) (*)
                new PushButton(Device, Device.Pins.D15)
            };

            for(int i = 0; i < pushButtons.Count; i++)
            {
                pushButtons[i].Clicked += PushbuttonClicked;
            }

            Console.WriteLine("PushButton ready!!!..");
        }

        void PushbuttonClicked(object sender, EventArgs e)
        {
            Console.WriteLine($"PushButton Clicked!");
        }
    }
}