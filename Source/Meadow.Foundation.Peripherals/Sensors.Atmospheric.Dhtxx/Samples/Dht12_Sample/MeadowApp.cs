using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.Dht12_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Dht12? sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            sensor = new Dht12(Device.CreateI2cBus());

            var consumer = Dht12.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                filter: result =>
                {
                    if (result.Old?.Temperature is { } oldTemp &&
                        result.Old?.Humidity is { } oldHumidity &&
                        result.New.Temperature is { } newTemp &&
                        result.New.Humidity is { } newHumidity)
                    {
                        return ((newTemp - oldTemp).Abs().Celsius > 0.5 &&
                                (newHumidity - oldHumidity).Percent > 0.05);
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity)> e) =>
            {
                Resolver.Log.Info($"  Temperature: {e.New.Temperature?.Celsius:N2}C");
                Resolver.Log.Info($"  Relative Humidity: {e.New.Humidity:N2}%");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            if (sensor == null) { return; }

            var conditions = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Resolver.Log.Info($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}