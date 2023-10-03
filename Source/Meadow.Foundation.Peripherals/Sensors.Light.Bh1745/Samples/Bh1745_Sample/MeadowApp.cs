using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Light;
using Meadow.Peripherals.Leds;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Bh1745 sensor;
        RgbPwmLed rgbLed;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Bh1745(Device.CreateI2cBus());

            // instantiate our onboard LED that we'll show the color with
            rgbLed = new RgbPwmLed(
                Device.Pins.OnboardLedRed,
                Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue,
                commonType: CommonType.CommonAnode);

            // Example that uses an IObservable subscription to only be notified
            var consumer = Bh1745.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: filter satisfied: {result.New.AmbientLight?.Lux:N2}Lux, old: {result.Old?.AmbientLight?.Lux:N2}Lux"),

                // only notify if the visible light changes by 100 lux (put your hand over the sensor to trigger)
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        // returns true if > 100lux change
                        return ((result.New.AmbientLight.Value - old.AmbientLight.Value).Abs().Lux > 100);
                    }
                    return false;
                });

            sensor.Subscribe(consumer);

            //classical .NET events can also be used:
            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"  Ambient Light: {result.New.AmbientLight?.Lux:N2}Lux");
                Resolver.Log.Info($"  Color: {result.New.Color}");

                if (result.New.Color is { } color)
                {
                    rgbLed.SetColor(color);
                }
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var result = await sensor.Read();

            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($" Visible Light: {result.AmbientLight?.Lux:N2}Lux");
            Resolver.Log.Info($" Color: {result.Color}");

            if (result.Color is { } color)
            {
                rgbLed.SetColor(color);
            }

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}