using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using System;
using System.Drawing;
using System.Threading;

namespace RgbLed_Sample
{
    class RgbLedApp : AppBase<F7Micro, RgbLedApp>
    {
        RgbLed rgbLed;

        public RgbLedApp()
        {
            rgbLed = new RgbLed(
                Device.CreateDigitalOutputPort(Device.Pins.D08),
                Device.CreateDigitalOutputPort(Device.Pins.D07),
                Device.CreateDigitalOutputPort(Device.Pins.D06));

            TestRgbLed();
        }

        protected void TestRgbLed()
        {
            while (true)
            {
                rgbLed.SetColor(Color.Red);
                Console.WriteLine("Red");
                Thread.Sleep(1000);

                rgbLed.SetColor(Color.Green);
                Console.WriteLine("Green");
                Thread.Sleep(1000);

                rgbLed.SetColor(Color.Blue);
                Console.WriteLine("Blue");
                Thread.Sleep(1000);
            }
        }
    }
}