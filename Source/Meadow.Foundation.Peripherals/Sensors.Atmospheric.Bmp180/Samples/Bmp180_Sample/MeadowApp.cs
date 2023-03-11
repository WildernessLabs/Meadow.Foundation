using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.Bmp180_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Bmp180 sensor;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initializing...");

            sensor = new Bmp180(Device.CreateI2cBus());

            var consumer = Bmp180.CreateObserver(
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
                        (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5);
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
                Resolver.Log.Info($"  Pressure: {result.New.Pressure?.Bar:N2}bar");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var conditions = await sensor.Read();
            Resolver.Log.Info($"Temperature: {conditions.Temperature?.Celsius}°C, Pressure: {conditions.Pressure?.Pascal}Pa");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}