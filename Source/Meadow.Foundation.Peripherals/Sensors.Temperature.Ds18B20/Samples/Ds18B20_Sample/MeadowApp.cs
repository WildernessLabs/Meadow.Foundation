using Meadow;
using Meadow.Devices;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Ds18B20_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>
        /*
        Ds18B20 ds18B20;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            ds18B20 = new ds18B20(Device.CreateI2cBus());

            var consumer = ds18B20.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Temperature New Value { result.New.Celsius}C");
                    Console.WriteLine($"Temperature Old Value { result.Old?.Celsius}C");
                },
                filter: null
            );
            ds18B20.Subscribe(consumer);

            ds18B20.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Console.WriteLine($"Temperature Updated: {e.New.Celsius:n2}C");
            };
            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var temp = await ds18B20.Read();
            Console.WriteLine($"Temperature New Value {temp.Celsius}C");

            ds18B20.StartUpdating();
        }*/

        //<!=SNOP=>
    }
}
