using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Peripherals.Leds;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    /// <summary>
    /// This sample illustrates basic push button usage. To use, add a button that
    /// terminates on the `3V3` rail on one end, and `D02` on the other, such
    /// that when the button is pressed, `D02` is raised `HIGH`.
    /// </summary>
    public class MeadowApp : App<F7FeatherV2>
    {
        RgbPwmLed onboardLed;
        PushButton pushButton;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            // intialize the push button
            pushButton = new PushButton(
                Device.Pins.D02,
                Meadow.Hardware.ResistorMode.InternalPullDown);

            //---- wire up the Classic .NET events
            // `PressStarted`
            pushButton.PressStarted += (s, e) =>
            {
                Resolver.Log.Info("pushButton.PressStarted.");
                onboardLed.SetColor(WildernessLabsColors.AzureBlue);
            };
            // `PressEnded`
            pushButton.PressEnded += (s, e) =>
            {
                Resolver.Log.Info("pushButton.PressEnded.");
                onboardLed.IsOn = false;
            };
            // `Clicked`
            pushButton.Clicked += (s, e) =>
            {
                Resolver.Log.Info("pushButton.Clicked.");
                onboardLed.SetColor(WildernessLabsColors.PearGreen);
                Thread.Sleep(250);
                onboardLed.IsOn = false;
            };
            // `LongPressClicked`
            pushButton.LongClicked += (s, e) =>
            {
                Resolver.Log.Info("pushButton.LongClicked.");
                onboardLed.SetColor(WildernessLabsColors.ChileanFire);
                Thread.Sleep(1000);
                onboardLed.IsOn = false;
            };

            Resolver.Log.Info("Hardware initialized.");

            return Task.CompletedTask;
        }
    }
}