using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;
using System;
using System.Threading.Tasks;

namespace Sensors.Light.Tsl2591_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Tsl2591 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            // configure our sensor on the I2C Bus
            var i2c = Device.CreateI2cBus();
            sensor = new Tsl2591(i2c);

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Tsl2591.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: filter satisfied: {result.New.VisibleLight?.Lux:N2}Lux, old: {result.Old?.VisibleLight?.Lux:N2}Lux"),
                // only notify if the visible light changes by 100 lux (put your hand over the sensor to trigger)
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        // returns true if > 100lux change
                        return ((result.New.VisibleLight.Value - old.VisibleLight.Value).Abs().Lux > 100);
                    }
                    return false;
                });
            sensor.Subscribe(consumer);

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"  Integrated Light: {result.New.Lux:N2}Lux");
            };

            sensor.InfraredUpdated += (sender, result) =>
            {
                Resolver.Log.Info($"  Infrared Light: {result.New.Lux:N2}Lux");
            };

            sensor.VisibleLightUpdated += (sender, result) =>
            {
                Resolver.Log.Info($"  Visible Light: {result.New.Lux:N2}Lux");
            };

            sensor.FullSpectrumUpdated += (sender, result) =>
            {
                Resolver.Log.Info($"  Full Spectrum Light: {result.New.Lux:N2}Lux");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var result = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  Full Spectrum Light: {result.FullSpectrum?.Lux:N2}Lux");
            Resolver.Log.Info($"  Infrared Light: {result.Infrared?.Lux:N2}Lux");
            Resolver.Log.Info($"  Visible Light: {result.VisibleLight?.Lux:N2}Lux");
            Resolver.Log.Info($"  Integrated Light: {result.Integrated?.Lux:N2}Lux");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}