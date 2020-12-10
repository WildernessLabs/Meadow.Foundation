using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using System;
using System.Collections.Generic;
using System.Threading;

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

            // Important note: You can only use on Push Button per Group Set (GSXX)
            pushButtons = new List<PushButton> 
            {
                new PushButton(Device, Device.Pins.A04, Meadow.Hardware.ResistorMode.PullUp),         // <- GS00
                //new PushButton(Device, Device.Pins.D06, Meadow.Hardware.ResistorMode.PullUp),         // <- GS00

                new PushButton(Device, Device.Pins.A05, Meadow.Hardware.ResistorMode.PullUp),         // <- GS01
                //new PushButton(Device, Device.Pins.D09, Meadow.Hardware.ResistorMode.PullUp),         // <- GS01

                new PushButton(Device, Device.Pins.A02, Meadow.Hardware.ResistorMode.PullUp),         // <- GS03
                //new PushButton(Device, Device.Pins.D14, Meadow.Hardware.ResistorMode.PullUp),         // <- GS03
                //new PushButton(Device, Device.Pins.D15, Meadow.Hardware.ResistorMode.PullUp)          // <- GS03

                new PushButton(Device, Device.Pins.A00, Meadow.Hardware.ResistorMode.PullUp),         // <- GS04

                new PushButton(Device, Device.Pins.A01, Meadow.Hardware.ResistorMode.PullUp),         // <- GS05
                //new PushButton(Device, Device.Pins.ESP_MOSI, Meadow.Hardware.ResistorMode.PullUp),    // <- GS05

                new PushButton(Device, Device.Pins.D02, Meadow.Hardware.ResistorMode.PullUp),         // <- GS06
                //new PushButton(Device, Device.Pins.D08, Meadow.Hardware.ResistorMode.PullUp),         // <- GS06

                new PushButton(Device, Device.Pins.A03, Meadow.Hardware.ResistorMode.PullUp),         // <- GS07
                //new PushButton(Device, Device.Pins.D05, Meadow.Hardware.ResistorMode.PullUp),         // <- GS07
                //new PushButton(Device, Device.Pins.D07, Meadow.Hardware.ResistorMode.PullUp),         // <- GS07

                new PushButton(Device, Device.Pins.D03, Meadow.Hardware.ResistorMode.PullUp),         // <- GS08

                new PushButton(Device, Device.Pins.D00, Meadow.Hardware.ResistorMode.PullUp),         // <- GS09
                //new PushButton(Device, Device.Pins.D04, Meadow.Hardware.ResistorMode.PullUp),         // <- GS09
                //new PushButton(Device, Device.Pins.D11, Meadow.Hardware.ResistorMode.PullUp),         // <- GS09

                new PushButton(Device, Device.Pins.D10, Meadow.Hardware.ResistorMode.PullUp),         // <- GS10
                
                new PushButton(Device, Device.Pins.ESP_MISO, Meadow.Hardware.ResistorMode.PullUp),    // <- GS11

                new PushButton(Device, Device.Pins.D01, Meadow.Hardware.ResistorMode.PullUp),         // <- GS13
                
                new PushButton(Device, Device.Pins.D12, Meadow.Hardware.ResistorMode.PullUp),         // <- GS14

                new PushButton(Device, Device.Pins.D13, Meadow.Hardware.ResistorMode.PullUp),         // <- GS15
            };

            for(int i = 0; i < pushButtons.Count; i++)
            {
                pushButtons[i].Clicked += PushbuttonClicked;
                //pushButtons[i].PressStarted += PushbuttonPressStarted;
                //pushButtons[i].PressEnded += PushbuttonPressEnded;
            }

            Console.WriteLine("PushButton(s) ready!!!");
        }

        void PushbuttonClicked(object sender, EventArgs e)
        {
            Console.WriteLine($"PushButton Clicked!");
            led.SetColor(RgbLed.Colors.Magenta);
            Thread.Sleep(100);
            led.SetColor(RgbLed.Colors.Red);
        }

        void PushbuttonPressStarted(object sender, EventArgs e)
        {
            Console.WriteLine($"PushButton PressStarted!");
            led.SetColor(RgbLed.Colors.Green);
        }

        void PushbuttonPressEnded(object sender, EventArgs e)
        {
            Console.WriteLine($"PushButton PressEnded!");
            led.SetColor(RgbLed.Colors.Red);
        }
    }
}