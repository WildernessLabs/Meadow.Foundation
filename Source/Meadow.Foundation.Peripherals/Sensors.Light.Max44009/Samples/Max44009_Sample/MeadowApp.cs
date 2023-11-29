using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;
using System;
using System.Threading.Tasks;

namespace Sensors.Light.Max44009_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Max44009 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Max44009(Device.CreateI2cBus());

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Max44009.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: filter satisfied: {result.New.Lux:N2}Lux, old: {result.Old?.Lux:N2}Lux"),

                // only notify if the visible light changes by 100 lux (put your hand over the sensor to trigger)
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        // returns true if > 100lux change
                        return (result.New - old).Abs().Lux > 100;
                    }
                    return false;
                });

            sensor.Subscribe(consumer);

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => Resolver.Log.Info($"Light: {result.New.Lux:N2}Lux");

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var result = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($" Light: {result.Lux:N2}Lux");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}