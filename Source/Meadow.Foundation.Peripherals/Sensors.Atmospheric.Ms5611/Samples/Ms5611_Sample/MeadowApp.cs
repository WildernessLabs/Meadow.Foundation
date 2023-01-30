using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ms5611_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ms5611 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            sensor = new Ms5611(Device.CreateI2cBus());

            var consumer = Ms5611.CreateObserver(
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
                        (result.New.Pressure.Value - old.Pressure.Value).Millibar > 0.5
                        );
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) => {
                Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
                Resolver.Log.Info($"  Pressure: {result.New.Pressure?.Millibar:N2}mbar ({result.New.Pressure?.Pascal:N2}Pa)");
            };

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            var conditions = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($" Temperature: {conditions.Temperature?.Celsius:N2}C");
            Resolver.Log.Info($" Pressure: {conditions.Pressure?.Bar:N2}hPa");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}
