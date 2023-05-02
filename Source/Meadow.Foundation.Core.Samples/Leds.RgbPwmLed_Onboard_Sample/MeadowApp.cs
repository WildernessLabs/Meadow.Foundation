using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leds.RgbPwmLed_Onboard_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        RgbLed onboardLed;

        public override Task Initialize()
        {
            Resolver.Log.Info("Creating peripherals...");

            onboardLed = new RgbLed(
                Device.Pins.OnboardLedRed,
                Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            TestColors();

            return Task.CompletedTask;
        }

        public async Task TestColors()
        {
            while (true)
            {
                Console.WriteLine("SetColor(RgbLedColors.Red);");
                onboardLed.SetColor(RgbLedColors.Red);
                Thread.Sleep(3000);

                Console.WriteLine("StartBlink();");
                await onboardLed.StartBlink();
                Thread.Sleep(3000);

                Console.WriteLine("StartBlink(RgbLedColors.Green);");
                await onboardLed.StartBlink(RgbLedColors.Green);
                Thread.Sleep(3000);

                Console.WriteLine("SetColor(RgbLedColors.Yellow);");
                onboardLed.SetColor(RgbLedColors.Yellow);
                Thread.Sleep(3000);

                Console.WriteLine("StartBlink(RgbLedColors.Cyan, 200, 200);");
                await onboardLed.StartBlink(RgbLedColors.Cyan, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200));
                Thread.Sleep(3000);

                await onboardLed.StopAnimation();
            }
        }

        //<!=SNOP=>
    }
}