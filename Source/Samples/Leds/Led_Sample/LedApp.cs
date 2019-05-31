using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using System;
using System.Threading;

namespace Led_Sample
{
    public class LedApp : App<F7Micro, LedApp>
    {
        Led led;

        public LedApp()
        {
            led = new Led(Device.CreateDigitalOutputPort(Device.Pins.D15, false));

            TestLed();
        }

        protected void TestLed()
        {
            while (true)
            {
                Console.WriteLine("Turning the LED on for a bit.");
                led.IsOn = true;
                Thread.Sleep(1000);
                led.IsOn = false;

                Console.WriteLine("Blinking the LED for a bit.");
                led.StartBlink();
                Thread.Sleep(1000);
                led.Stop();
            }
        }
    }
}