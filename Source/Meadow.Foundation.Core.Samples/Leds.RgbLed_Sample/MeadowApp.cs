using System;
using System.Collections.Generic;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;

namespace Leds.RgbLed_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        protected List<RgbLed> rgbLeds;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var onRgbLed = new RgbLed(
                device: Device,
                redPin: Device.Pins.OnboardLedRed,
                greenPin: Device.Pins.OnboardLedGreen,
                bluePin: Device.Pins.OnboardLedBlue);
            onRgbLed.SetColor(RgbLed.Colors.Red);

            rgbLeds = new List<RgbLed>
            {
                new RgbLed(
                    Device.CreateDigitalOutputPort(Device.Pins.D02),
                    Device.CreateDigitalOutputPort(Device.Pins.D03),
                    Device.CreateDigitalOutputPort(Device.Pins.D04)),
                new RgbLed(
                    Device.CreateDigitalOutputPort(Device.Pins.D05),
                    Device.CreateDigitalOutputPort(Device.Pins.D06),
                    Device.CreateDigitalOutputPort(Device.Pins.D07)),
                new RgbLed(
                    Device.CreateDigitalOutputPort(Device.Pins.D08),
                    Device.CreateDigitalOutputPort(Device.Pins.D09),
                    Device.CreateDigitalOutputPort(Device.Pins.D10)),
                new RgbLed(
                    Device.CreateDigitalOutputPort(Device.Pins.D11),
                    Device.CreateDigitalOutputPort(Device.Pins.D12),
                    Device.CreateDigitalOutputPort(Device.Pins.D13))
            };

            onRgbLed.SetColor(RgbLed.Colors.Green);

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

                Thread.Sleep(1000);

                Console.WriteLine("Blinking through each color on each RGB LED...");
                foreach (var rgbLed in rgbLeds)
                {
                    for (int i = 0; i < (int)RgbLed.Colors.count; i++)
                    {
                        rgbLed.StartBlink((RgbLed.Colors)i);
                        Thread.Sleep(3000);
                    }
                }

                Thread.Sleep(1000);

                Console.WriteLine("Blinking through each color on each RGB LED...");
                foreach (var rgbLed in rgbLeds)
                {
                    for (int i = 0; i < (int)RgbLed.Colors.count; i++)
                    {
                        rgbLed.StartBlink((RgbLed.Colors)i, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                        Thread.Sleep(3000);
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}