using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.AirQuality.Ccs811_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ccs811 sensor;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initializing...");

            var i2c = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Fast);
            sensor = new Ccs811(i2c);

            var consumer = Ccs811.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer triggered:");
                    Resolver.Log.Info($"   new CO2: {result.New.Co2?.PartsPerMillion:N1}ppm, old: {result.Old?.Co2?.PartsPerMillion:N1}ppm.");
                    Resolver.Log.Info($"   new VOC: {result.New.Voc?.PartsPerBillion:N1}ppb, old: {result.Old?.Voc?.PartsPerBillion:N1}ppb.");
                },
                filter: result =>
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
                    if (result.Old is { } old)
                    {
                        return (
                        (result.New.Co2.Value - old.Co2.Value).Abs().PartsPerMillion > 1000 // 1000ppm
                          &&
                        (result.New.Voc.Value - old.Voc.Value).Abs().PartsPerBillion > 100 // 100ppb
                        );
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"CO2: {result.New.Co2.Value.PartsPerMillion:n1}ppm, VOC: {result.New.Voc.Value.PartsPerBillion:n1}ppb");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var result = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  CO2: {result.Co2.Value.PartsPerMillion:n1}ppm");
            Resolver.Log.Info($"  VOC: {result.Voc.Value.PartsPerBillion:n1}ppb");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}