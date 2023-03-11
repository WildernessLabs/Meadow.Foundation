using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace Sensors.Light.Veml7700_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Veml7700 sensor;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Veml7700(Device.CreateI2cBus());
            sensor.DataSource = Veml7700.SensorTypes.Ambient;
            
            sensor.RangeExceededHigh += (s, a) => Resolver.Log.Info("Too bright to measure");
            sensor.RangeExceededLow += (s, a) => Resolver.Log.Info("Too dim to measure");

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => Resolver.Log.Info($"Illuminance: {result.New.Lux:n3}Lux");

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var conditions = await sensor.Read();

            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  Illuminance: {conditions.Lux:n3}Lux");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }
    
        //<!=SNOP=>
    }
}