using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Mcp9808_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mcp9808 mcp9808;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            mcp9808 = new Mcp9808(Device.CreateI2cBus());

            var consumer = Mcp9808.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Temperature New Value { result.New.Celsius}C");
                    Console.WriteLine($"Temperature Old Value { result.Old?.Celsius}C");
                },
                filter: null
            );
            mcp9808.Subscribe(consumer);

            mcp9808.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Console.WriteLine($"Temperature Updated: {e.New.Celsius:N2}C");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var temp = await mcp9808.Read();

            Console.WriteLine($"Temperature New Value {temp.Celsius}C");

            mcp9808.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}