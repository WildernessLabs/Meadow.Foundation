using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using System;
using System.Threading.Tasks;

namespace Sensors.Environmental.Scd40_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Scd41 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            var i2cBus = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard);

            sensor = new Scd41(i2cBus);
            var serialNum = sensor.GetSerialNumber();
            Resolver.Log.Info($"Serial: {BitConverter.ToString(serialNum)}");

            var consumer = Scd41.CreateObserver(
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

            sensor?.Subscribe(consumer);

            if (sensor != null)
            {
                sensor.Updated += (sender, result) =>
                {
                    Resolver.Log.Info($"  Concentration: {result.New.Concentration?.PartsPerMillion:N0}ppm");
                    Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N1}C");
                    Resolver.Log.Info($"  Relative Humidity: {result.New.Humidity:N0}%");
                };
            }

            sensor?.StartUpdating(TimeSpan.FromSeconds(6));

            return base.Initialize();
        }

        //<!=SNOP=>
    }
}