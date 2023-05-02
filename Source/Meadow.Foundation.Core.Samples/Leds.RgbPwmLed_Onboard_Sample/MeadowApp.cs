using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leds.RgbPwmLed_Onboard_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        RgbPwmLed onboardLed;

        public override Task Initialize()
        {
            Resolver.Log.Info("Creating peripherals...");

            onboardLed = new RgbPwmLed(
                Device.Pins.OnboardLedRed,
                Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            _ = TestColors();

            return Task.CompletedTask;
        }

        public async Task TestColors()
        {
            while (true)
            {
                Console.WriteLine("SetColor(RgbLedColors.Red);");
                onboardLed.SetColor(Color.Red);
                Thread.Sleep(3000);

                Console.WriteLine("StartPulse();");
                await onboardLed.StartPulse();
                Thread.Sleep(3000);

                Console.WriteLine("StartPulse(RgbLedColors.Green);");
                await onboardLed.StartPulse(Color.Green);
                Thread.Sleep(3000);

                Console.WriteLine("SetColor(RgbLedColors.Yellow);");
                onboardLed.SetColor(Color.Yellow);
                Thread.Sleep(3000);

                Console.WriteLine("StartPulse(RgbLedColors.Cyan, 200, 200);");
                await onboardLed.StartPulse(Color.Cyan, TimeSpan.FromMilliseconds(400));
                Thread.Sleep(3000);

                await onboardLed.StopAnimation();
            }
        }

        //<!=SNOP=>
    }
}