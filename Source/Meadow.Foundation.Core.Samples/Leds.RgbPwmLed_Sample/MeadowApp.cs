using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
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

            rgbPwmLeds = new List<RgbPwmLed>()
            {
                new RgbPwmLed(
                    Device.Pins.D02,
                    Device.Pins.D03,
                    Device.Pins.D04),
                //new RgbPwmLed(
                //    Device.Pins.D05,
                //    Device.Pins.D06,
                //    Device.Pins.D07),
                new RgbPwmLed(
                    Device.Pins.D08,
                    Device.Pins.D09,
                    Device.Pins.D10),
                new RgbPwmLed(
                    Device.Pins.D11,
                    Device.Pins.D12,
                    Device.Pins.D13)
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
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
                    await rgbPwmLed.StopAnimation();

                    for (int i = 0; i < 10; i++)
                    {
                        rgbPwmLed.SetColor(Color.Green, i * 0.1f);
                        Resolver.Log.Info($"Green brightness: {i * 0.1f}");
                        Thread.Sleep(500);
                    }
                    await rgbPwmLed.StopAnimation();

                    for (int i = 0; i < 10; i++)
                    {
                        rgbPwmLed.SetColor(Color.Blue, i * 0.1f);
                        Resolver.Log.Info($"Blue brightness: {i * 0.1f}");
                        Thread.Sleep(500);
                    }
                    await rgbPwmLed.StopAnimation();

                    // Blink
                    await rgbPwmLed.StartBlink(Color.Red);
                    Resolver.Log.Info("Blinking Red");
                    Thread.Sleep(3000);

                    await rgbPwmLed.StartBlink(Color.Green);
                    Resolver.Log.Info("Blinking Green");
                    Thread.Sleep(3000);

                    await rgbPwmLed.StartBlink(Color.Blue);
                    Resolver.Log.Info("Blinking Blue");
                    Thread.Sleep(3000);

                    // Pulse
                    await rgbPwmLed.StartPulse(Color.Red);
                    Resolver.Log.Info("Pulsing Red");
                    Thread.Sleep(3000);

                    await rgbPwmLed.StartPulse(Color.Green);
                    Resolver.Log.Info("Pulsing Green");
                    Thread.Sleep(3000);

                    await rgbPwmLed.StartPulse(Color.Blue);
                    Resolver.Log.Info("Pulsing Blue");
                    Thread.Sleep(3000);
                }
            }
        }

        //<!=SNOP=>
    }
}