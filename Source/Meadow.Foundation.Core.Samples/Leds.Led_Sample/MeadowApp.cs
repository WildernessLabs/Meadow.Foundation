using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Leds.Led_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        readonly List<Led> leds;

        public MeadowApp()
        {
            var onRgbLed = new RgbLed(
                device: Device,
                redPin: Device.Pins.OnboardLedRed,
                greenPin: Device.Pins.OnboardLedGreen,
                bluePin: Device.Pins.OnboardLedBlue);
            onRgbLed.SetColor(RgbLed.Colors.Red);

            leds = new List<Led>
            {
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D00, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D01, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D02, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D03, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D04, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D05, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D06, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D07, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D08, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D09, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D10, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D11, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D12, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D13, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D14, false)),
                new Led(Device.CreateDigitalOutputPort(Device.Pins.D15, false))
            };

            onRgbLed.SetColor(RgbLed.Colors.Green);

            TestLeds();
        }

        protected void TestLeds()
        {
            Console.WriteLine("TestLeds...");

            while (true)
            {
                Console.WriteLine("Turning on each led every 100ms");
                foreach (var led in leds)
                {
                    led.IsOn = true;
                    Thread.Sleep(100);
                }

                Console.WriteLine("Turning off each led every 100ms");
                foreach (var led in leds)
                {
                    led.IsOn = false;
                    Thread.Sleep(100);
                }

                Console.WriteLine("Turning on and off each led for 100ms");
                foreach (var led in leds)
                {
                    led.IsOn = true;
                    Thread.Sleep(100);
                    led.IsOn = false;
                }

                Console.WriteLine("Blinking the LEDs for a second each");
                foreach (var led in leds)
                {
                    led.StartBlink();
                    Thread.Sleep(1000);
                    led.Stop();
                }
            }
        }
    }
}