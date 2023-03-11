using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;

namespace Leds.RgbLed_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected List<RgbLed> rgbLeds;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initializing...");

            var onRgbLed = new RgbLed(
                redPin: Device.Pins.OnboardLedRed,
                greenPin: Device.Pins.OnboardLedGreen,
                bluePin: Device.Pins.OnboardLedBlue);
            onRgbLed.SetColor(RgbLedColors.Red);

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

            onRgbLed.SetColor(RgbLedColors.Green);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("TestRgbLeds...");

            while (true)
            {
                Resolver.Log.Info("Going through each color on each RGB LED...");
                foreach (var rgbLed in rgbLeds)
                {
                    for (int i = 0; i < (int)RgbLedColors.count; i++)
                    {
                        rgbLed.SetColor((RgbLedColors)i);
                        await Task.Delay(500);
                    }
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Blinking through each color on each RGB LED...");
                foreach (var rgbLed in rgbLeds)
                {
                    for (int i = 0; i < (int)RgbLedColors.count; i++)
                    {
                        rgbLed.StartBlink((RgbLedColors)i);
                        await Task.Delay(3000);
                    }
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Blinking through each color on each RGB LED...");
                foreach (var rgbLed in rgbLeds)
                {
                    for (int i = 0; i < (int)RgbLedColors.count; i++)
                    {
                        rgbLed.StartBlink((RgbLedColors)i, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                        await Task.Delay(3000);
                    }
                }

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}