using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leds.RgbPwmLed_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        List<RgbPwmLed> rgbPwmLeds;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            var onRgbLed = new RgbLed(
                device: Device,
                redPin: Device.Pins.OnboardLedRed,
                greenPin: Device.Pins.OnboardLedGreen,
                bluePin: Device.Pins.OnboardLedBlue);
            onRgbLed.SetColor(RgbLedColors.Red);

            rgbPwmLeds = new List<RgbPwmLed>()
            {
                new RgbPwmLed(
                    Device,
                    Device.Pins.D02,
                    Device.Pins.D03,
                    Device.Pins.D04),
                new RgbPwmLed(
                    Device,
                    Device.Pins.D05,
                    Device.Pins.D06,
                    Device.Pins.D07),
                new RgbPwmLed(
                    Device,
                    Device.Pins.D08,
                    Device.Pins.D09,
                    Device.Pins.D10),
                new RgbPwmLed(
                    Device,
                    Device.Pins.D11,
                    Device.Pins.D12,
                    Device.Pins.D13)
            };

            onRgbLed.SetColor(RgbLedColors.Green);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Info("TestRgbPwmLed...");

            while (true)
            {
                foreach (var rgbPwmLed in rgbPwmLeds)
                {
                    rgbPwmLed.SetColor(Color.Red);
                    Resolver.Log.Info("Red");
                    Thread.Sleep(1000);

                    rgbPwmLed.SetColor(Color.Green);
                    Resolver.Log.Info("Green");
                    Thread.Sleep(1000);

                    rgbPwmLed.SetColor(Color.Blue);
                    Resolver.Log.Info("Blue");
                    Thread.Sleep(1000);

                    for (int i = 0; i < 10; i++)
                    {
                        rgbPwmLed.SetColor(Color.Red, i * 0.1f);
                        Resolver.Log.Info($"Red brightness: {i * 0.1f}");
                        Thread.Sleep(500);
                    }
                    rgbPwmLed.Stop();

                    for (int i = 0; i < 10; i++)
                    {
                        rgbPwmLed.SetColor(Color.Green, i * 0.1f);
                        Resolver.Log.Info($"Green brightness: {i * 0.1f}");
                        Thread.Sleep(500);
                    }
                    rgbPwmLed.Stop();

                    for (int i = 0; i < 10; i++)
                    {
                        rgbPwmLed.SetColor(Color.Blue, i * 0.1f);
                        Resolver.Log.Info($"Blue brightness: {i * 0.1f}");
                        Thread.Sleep(500);
                    }
                    rgbPwmLed.Stop();

                    // Blink
                    rgbPwmLed.StartBlink(Color.Red, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), 1f, 0f);
                    Resolver.Log.Info("Blinking Red");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartBlink(Color.Green, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), 1f, 0f);
                    Resolver.Log.Info("Blinking Green");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartBlink(Color.Blue, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), 1f, 0f);
                    Resolver.Log.Info("Blinking Blue");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    // Pulse
                    rgbPwmLed.StartPulse(Color.Red);
                    Resolver.Log.Info("Pulsing Red");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartPulse(Color.Green);
                    Resolver.Log.Info("Pulsing Green");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();

                    rgbPwmLed.StartPulse(Color.Blue);
                    Resolver.Log.Info("Pulsing Blue");
                    Thread.Sleep(3000);
                    rgbPwmLed.Stop();
                }
            }
        }

        //<!=SNOP=>
    }
}