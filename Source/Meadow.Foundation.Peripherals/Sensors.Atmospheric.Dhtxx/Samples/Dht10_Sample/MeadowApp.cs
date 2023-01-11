using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Dht10_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        Dht10? dht10;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            dht10 = new Dht10(Device.CreateI2cBus());

            var consumer = Dht10.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                filter: result =>
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
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
            dht10.Subscribe(consumer);

            dht10.Updated += (object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity)> e) =>
            {
                Resolver.Log.Info($"  Temperature: {e.New.Temperature?.Celsius:N2}C");
                Resolver.Log.Info($"  Relative Humidity: {e.New.Humidity:N2}%");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            if(dht10 == null)
            {
                return;
            }

            var conditions = await dht10.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Resolver.Log.Info($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");

            dht10.StartUpdating(TimeSpan.FromSeconds(1));
        }
    }
}