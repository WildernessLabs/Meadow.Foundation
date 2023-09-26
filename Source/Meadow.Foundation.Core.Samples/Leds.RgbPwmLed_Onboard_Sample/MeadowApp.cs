using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using System;
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
            return TestColors();
        }

        public async Task TestColors()
        {
            while (true)
            {
                Resolver.Log.Info("SetColor(RgbLedColors.Red);");
                onboardLed.SetColor(Color.Red);
                await Task.Delay(3000);

                Resolver.Log.Info("StartPulse();");
                await onboardLed.StartPulse();
                await Task.Delay(3000);

                Resolver.Log.Info("StartPulse(RgbLedColors.Green);");
                await onboardLed.StartPulse(Color.Green);
                await Task.Delay(3000);

                Resolver.Log.Info("SetColor(RgbLedColors.Yellow);");
                onboardLed.SetColor(Color.Yellow);
                await Task.Delay(3000);

                Resolver.Log.Info("StartPulse(RgbLedColors.Cyan, 200, 200);");
                await onboardLed.StartPulse(Color.Cyan, TimeSpan.FromMilliseconds(400));
                await Task.Delay(3000);

                await onboardLed.StopAnimation();
            }
        }

        //<!=SNOP=>
    }
}