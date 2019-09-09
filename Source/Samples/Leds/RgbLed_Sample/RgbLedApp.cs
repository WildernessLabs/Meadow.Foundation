using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RgbLed_Sample
{
    class RgbLedApp : App<F7Micro, RgbLedApp>
    {
        protected List<RgbLed> rgbLeds;

        public RgbLedApp()
        {
            Console.WriteLine("Initializing...");

            rgbLeds = new List<RgbLed>();
            rgbLeds.Add(new RgbLed(
                Device.CreateDigitalOutputPort(Device.Pins.D02),
                Device.CreateDigitalOutputPort(Device.Pins.D03),
                Device.CreateDigitalOutputPort(Device.Pins.D04))
            );
            rgbLeds.Add(new RgbLed(
                Device.CreateDigitalOutputPort(Device.Pins.D05),
                Device.CreateDigitalOutputPort(Device.Pins.D06),
                Device.CreateDigitalOutputPort(Device.Pins.D07))
            );
            rgbLeds.Add(new RgbLed(
                Device.CreateDigitalOutputPort(Device.Pins.D08),
                Device.CreateDigitalOutputPort(Device.Pins.D09),
                Device.CreateDigitalOutputPort(Device.Pins.D10))
            );
            rgbLeds.Add(new RgbLed(
                Device.CreateDigitalOutputPort(Device.Pins.D11),
                Device.CreateDigitalOutputPort(Device.Pins.D12),
                Device.CreateDigitalOutputPort(Device.Pins.D13))
            );

            TestRgbLeds();
        }

        protected void TestRgbLeds()
        {
            Console.WriteLine("TestRgbLeds...");

            while (true)
            {
                Console.WriteLine("Going through each color on each RGB LED...");
                foreach (var rgbLed in rgbLeds)
                {
                    for (int i = 0; i < (int)RgbLed.Colors.count; i++)
                    {
                        rgbLed.SetColor((RgbLed.Colors)i);
                        Thread.Sleep(500);
                    }
                }

                Console.WriteLine("Blinking through each color on each RGB LED...");
                foreach (var rgbLed in rgbLeds)
                {
                    for (int i = 0; i < (int)RgbLed.Colors.count; i++)
                    {
                        rgbLed.StartBlink((RgbLed.Colors)i);
                        Thread.Sleep(3000);
                    }
                }
            }
        }
    }
}