using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using System;
using System.Threading;

namespace RgbLed_Sample
{
    class RgbLedApp : App<F7Micro, RgbLedApp>
    {
        RgbLed rgbLed;

        public RgbLedApp()
        {
            rgbLed = new RgbLed(
                Device.CreateDigitalOutputPort(Device.Pins.D02),
                Device.CreateDigitalOutputPort(Device.Pins.D03),
                Device.CreateDigitalOutputPort(Device.Pins.D04));

            TestRgbLed();
        }

        protected void TestRgbLed()
        {
            while (true)
            {
                for (int i = 0; i < (int)RgbLed.Colors.count; i++)
                {
                    rgbLed.SetColor((RgbLed.Colors)i);
                    Console.WriteLine(((RgbLed.Colors)i).ToString());
                    Thread.Sleep(1000);
                }

         /*       rgbLed.SetColor(RgbLed.Colors.Red);
                Console.WriteLine("Red");
                Thread.Sleep(1000);

                rgbLed.SetColor(RgbLed.Colors.Green);
                Console.WriteLine("Green");
                Thread.Sleep(1000);

                rgbLed.SetColor(RgbLed.Colors.Blue);
                Console.WriteLine("Blue");
                Thread.Sleep(1000); */
            }
        }
    }
}