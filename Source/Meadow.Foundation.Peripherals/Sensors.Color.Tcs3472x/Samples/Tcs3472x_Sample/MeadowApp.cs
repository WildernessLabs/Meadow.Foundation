using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Color;
using System;
using System.Threading.Tasks;

namespace Sensors.Light.Tcs3472x_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        private Tcs3472x sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            // configure our sensor on the I2C Bus
            sensor = new Tcs3472x(Device.CreateI2cBus());

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Tcs3472x.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: filter satisfied: {result.New}, old: {result.Old}"),
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return Math.Abs(result.New.R - old.R) > 50;
                    }
                    return false;
                });
            sensor.Subscribe(consumer);

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"  Color: {result.New}");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var result = await sensor.Read();
            Resolver.Log.Info($"Initial reading: {result}");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}