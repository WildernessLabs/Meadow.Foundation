using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Thermistor
{
    // TODO: This sample needs a rewrite. See the other atmospheric samples for
    // an example of the sample pattern.

    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private SteinhartHartCalculatedThermistor thermistor;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            thermistor = new SteinhartHartCalculatedThermistor(Device.CreateAnalogInputPort(Device.Pins.A00), new Resistance(10, Meadow.Units.Resistance.UnitType.Kiloohms));

            var consumer = SteinhartHartCalculatedThermistor.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Temperature New Value {result.New.Celsius}C");
                    Console.WriteLine($"Temperature Old Value {result.Old?.Celsius}C");
                },
                filter: null
            );
            thermistor.Subscribe(consumer);

            thermistor.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Console.WriteLine($"Temperature Updated: {e.New.Celsius:N2}C");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var temp = await thermistor.Read();
            Console.WriteLine($"Current temperature: {temp.Celsius} C");

            thermistor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}