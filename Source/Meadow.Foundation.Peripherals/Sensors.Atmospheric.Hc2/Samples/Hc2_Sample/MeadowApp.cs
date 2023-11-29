using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.HC2_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        HC2 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            // Analog
            //sensor = new HC2(Device.Pins.A00, Device.Pins.A01);

            // Serial
            sensor = new HC2(Device, Device.PlatformOS.GetSerialPortName("COM4"));

            var consumer = HC2.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new Temp: {result.New.Temperature?.Celsius:N2} °C, old: {result.Old?.Temperature?.Celsius:N2} °C");
                },
                filter: result =>
                {
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

            sensor.StartUpdating(TimeSpan.FromSeconds(5));
        }

        //<!=SNOP=>
    }
}