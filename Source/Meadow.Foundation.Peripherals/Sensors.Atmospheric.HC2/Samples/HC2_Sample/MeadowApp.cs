using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.HC2_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        HC2 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            // Analog
            sensor = new HC2(Device.Pins.A00, Device.Pins.A01);

            // Serial
            // TODO: Still needs testing/debugging
            //sensor = new HC2(Device, Device.PlatformOS.GetSerialPortName("COM4"));

            var consumer = HC2.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2} °C, old: {result.Old?.Temperature?.Celsius:N2} °C");
                },
                filter: result =>
                {
                    // C# 8 pattern match syntax. checks for !null and assigns var.
                    if (result.Old is { } old)
                    {
                        if (result.New.Temperature.HasValue && old.Temperature.HasValue)
                            return ((result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5);
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"Relative Humidity: {result.New.Humidity?.Percent:N2} %, Temperature: {result.New.Temperature?.Celsius:N2} °C");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info($"Initial Read:");
            var conditions = await sensor.Read();
            Resolver.Log.Info($"Relative Humidity: {conditions.Humidity?.Percent:N2} %, Temperature: {conditions.Temperature?.Celsius:N2} °C");

            Resolver.Log.Info($"StartUpdating()");
            sensor.StartUpdating(TimeSpan.FromSeconds(5));
        }

        //<!=SNOP=>
    }
}