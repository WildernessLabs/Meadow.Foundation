using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Hih6130? sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            sensor = new Hih6130(Device.CreateI2cBus());

            var consumer = Hih6130.CreateObserver(
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

            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"  Temperature: {result?.New.Temperature?.Celsius:F1}°C");
                Resolver.Log.Info($"  Relative Humidity: {result?.New.Humidity?.Percent:F1}%");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            if(sensor == null) { return; }

            var result = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  Temperature: {result.Temperature?.Celsius:F1}°C");
            Resolver.Log.Info($"  Relative Humidity: {result.Humidity:F1}%");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}