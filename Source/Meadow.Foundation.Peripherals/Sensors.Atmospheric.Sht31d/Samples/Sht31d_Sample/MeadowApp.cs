using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace BasicSensors.Atmospheric.SHT31D_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Sht31d sensor;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initializing...");

            sensor = new Sht31d(Device.CreateI2cBus());

            var consumer = Sht31d.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                filter: result =>
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
                    if (result.Old is { } old)
                    {
                        return (
                        (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5
                        &&
                        (result.New.Humidity.Value.Percent - old.Humidity.Value.Percent) > 0.05
                        );
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
                Resolver.Log.Info($"  Relative Humidity: {result.New.Humidity:N2}%");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var conditions = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Resolver.Log.Info($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}