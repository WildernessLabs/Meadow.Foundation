using System;
using System.Collections.Generic;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;

namespace Sensors.Buttons.PushButton_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbLed led;
        List<PushButton> pushButtons;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            led = new RgbLed(Device, Device.Pins.OnboardLedRed, Device.Pins.OnboardLedGreen, Device.Pins.OnboardLedBlue);
            led.SetColor(RgbLed.Colors.Red);

            pushButtons = new List<PushButton> 
            {
                new PushButton(Device, Device.Pins.D00, Meadow.Hardware.ResistorMode.PullUp), // <- GS09
                //new PushButton(Device, Device.Pins.D01, Meadow.Hardware.ResistorMode.PullUp), // <- GS
                new PushButton(Device, Device.Pins.D02, Meadow.Hardware.ResistorMode.PullUp), // <- GS06
                //new PushButton(Device, Device.Pins.D03, Meadow.Hardware.ResistorMode.PullUp), // <- GS08
                //new PushButton(Device, Device.Pins.D04, Meadow.Hardware.ResistorMode.PullUp), // <- GS09
                //new PushButton(Device, Device.Pins.D05, Meadow.Hardware.ResistorMode.PullUp), // <- GS07
                new PushButton(Device, Device.Pins.D06, Meadow.Hardware.ResistorMode.PullUp), // <- GS00
                //new PushButton(Device, Device.Pins.D07, Meadow.Hardware.ResistorMode.PullUp), // <- GS07
                //new PushButton(Device, Device.Pins.D08, Meadow.Hardware.ResistorMode.PullUp), // <- GS06
                new PushButton(Device, Device.Pins.D09, Meadow.Hardware.ResistorMode.PullUp), // <- GS01
                //new PushButton(Device, Device.Pins.D10, Meadow.Hardware.ResistorMode.PullUp), // <- GS
                //new PushButton(Device, Device.Pins.D11, Meadow.Hardware.ResistorMode.PullUp), // <- GS09
                new PushButton(Device, Device.Pins.D12, Meadow.Hardware.ResistorMode.PullUp), // <- GS14
                //new PushButton(Device, Device.Pins.D13, Meadow.Hardware.ResistorMode.PullUp), // <- GS15
                new PushButton(Device, Device.Pins.D14, Meadow.Hardware.ResistorMode.PullUp), // <- GS03
                //new PushButton(Device, Device.Pins.D15, Meadow.Hardware.ResistorMode.PullUp)  // <- GS03
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
            led.SetColor(RgbLed.Colors.Green);
            Thread.Sleep(100);
            led.SetColor(RgbLed.Colors.Red);
        }
    }
}