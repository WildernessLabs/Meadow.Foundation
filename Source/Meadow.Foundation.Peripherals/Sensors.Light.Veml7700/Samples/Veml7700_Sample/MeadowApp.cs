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

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            sensor = new Veml7700(Device.CreateI2cBus());
            sensor.DataSource = Veml7700.SensorTypes.Ambient;
            
            sensor.RangeExceededHigh += (s, a) => Console.WriteLine("Too bright to measure");
            sensor.RangeExceededLow += (s, a) => Console.WriteLine("Too dim to measure");

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => Console.WriteLine($"Illuminance: {result.New.Lux:n3}Lux");

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var conditions = await sensor.Read();

            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Illuminance: {conditions.Lux:n3}Lux");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }
    
        //<!=SNOP=>
    }
}