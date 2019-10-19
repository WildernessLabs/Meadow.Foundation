using System;
using System.Collections.Generic;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;

namespace Leds.RgbPwmLed_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        List<RgbPwmLed> rgbPwmLeds;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            rgbPwmLeds = new List<RgbPwmLed>()
            {
                new RgbPwmLed(
                    Device.CreatePwmPort(Device.Pins.D04),
                    Device.CreatePwmPort(Device.Pins.D03),
                    Device.CreatePwmPort(Device.Pins.D02)),
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

            TestRgbPwmLed();
        }

        protected void TestRgbPwmLed()
        {
            Console.WriteLine("TestRgbPwmLed...");

            while (true)
            {
                //for (int i = 0; i < (int)RgbLed.Colors.count; i++)
                //{
                //    rgbLed.SetColor((RgbLed.Colors)i);
                //    Console.WriteLine(((RgbLed.Colors)i).ToString());
                //    Thread.Sleep(1000);
                //}

                foreach (var rgbPwmLed in rgbPwmLeds)
                {
                    // SetColor
                    rgbPwmLed.SetColor(Color.Red);
                    Console.WriteLine("Red");
                    Thread.Sleep(1000);

                    rgbPwmLed.SetColor(Color.Green);
                    Console.WriteLine("Green");
                    Thread.Sleep(1000);                    

                    rgbPwmLed.SetColor(Color.Blue);
                    Console.WriteLine("Blue");
                    Thread.Sleep(1000);                    

                    // Blink
                    rgbPwmLed.StartBlink(Color.Red, 500, 500, 0.65f, 0.25f);
                    Console.WriteLine("Blinking Red");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartBlink(Color.Green, 500, 500, 0.65f, 0.25f);
                    Console.WriteLine("Blinking Green");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartBlink(Color.Blue, 500, 500, 0.65f, 0.25f);
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