using System;
using System.Collections.Generic;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;

namespace Leds.RgbPwmLed_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        List<RgbPwmLed> rgbPwmLeds;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var onRgbLed = new RgbLed(
                device: Device,
                redPin: Device.Pins.OnboardLedRed,
                greenPin: Device.Pins.OnboardLedGreen,
                bluePin: Device.Pins.OnboardLedBlue);
            onRgbLed.SetColor(RgbLed.Colors.Red);

            rgbPwmLeds = new List<RgbPwmLed>()
            {
                new RgbPwmLed(
                    Device.CreatePwmPort(Device.Pins.D02),
                    Device.CreatePwmPort(Device.Pins.D03),
                    Device.CreatePwmPort(Device.Pins.D04)),
                new RgbPwmLed(
                    Device.CreatePwmPort(Device.Pins.D05),
                    Device.CreatePwmPort(Device.Pins.D06),
                    Device.CreatePwmPort(Device.Pins.D07)),
                new RgbPwmLed(
                    Device.CreatePwmPort(Device.Pins.D08),
                    Device.CreatePwmPort(Device.Pins.D09),
                    Device.CreatePwmPort(Device.Pins.D10)),
                new RgbPwmLed(
                    Device.CreatePwmPort(Device.Pins.D11),
                    Device.CreatePwmPort(Device.Pins.D12),
                    Device.CreatePwmPort(Device.Pins.D13))
            };

            onRgbLed.SetColor(RgbLed.Colors.Green);

            TestRgbPwmLed();
        }

        protected void TestRgbPwmLed()
        {
            Console.WriteLine("TestRgbPwmLed...");

            while (true)
            {
                foreach (var rgbPwmLed in rgbPwmLeds)
                {
                    rgbPwmLed.SetColor(Color.Red);
                    Console.WriteLine("Red");
                    Thread.Sleep(1000);

                    rgbPwmLed.SetColor(Color.Green);
                    Console.WriteLine("Green");
                    Thread.Sleep(1000);

                    rgbPwmLed.SetColor(Color.Blue);
                    Console.WriteLine("Blue");
                    Thread.Sleep(1000);

                    for (int i = 0; i < 10; i++)
                    {
                        rgbPwmLed.SetColor(Color.Red, i * 0.1f);
                        Console.WriteLine($"Red brightness: {i * 0.1f}");
                        Thread.Sleep(500);
                    }
                    rgbPwmLed.Stop();

                    for (int i = 0; i < 10; i++)
                    {
                        rgbPwmLed.SetColor(Color.Green, i * 0.1f);
                        Console.WriteLine($"Green brightness: {i * 0.1f}");
                        Thread.Sleep(500);
                    }
                    rgbPwmLed.Stop();

                    for (int i = 0; i < 10; i++)
                    {
                        rgbPwmLed.SetColor(Color.Blue, i * 0.1f);
                        Console.WriteLine($"Blue brightness: {i * 0.1f}");
                        Thread.Sleep(500);
                    }
                    rgbPwmLed.Stop();

                    // Blink
                    rgbPwmLed.StartBlink(Color.Red, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), 1f, 0f);
                    Console.WriteLine("Blinking Red");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartBlink(Color.Green, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), 1f, 0f);
                    Console.WriteLine("Blinking Green");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartBlink(Color.Blue, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), 1f, 0f);
                    Console.WriteLine("Blinking Blue");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    // Pulse
                    rgbPwmLed.StartPulse(Color.Red);
                    Console.WriteLine("Pulsing Red");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartPulse(Color.Green);
                    Console.WriteLine("Pulsing Green");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartPulse(Color.Blue);
                    Console.WriteLine("Pulsing Blue");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();
                }
            }
        }
    }
}