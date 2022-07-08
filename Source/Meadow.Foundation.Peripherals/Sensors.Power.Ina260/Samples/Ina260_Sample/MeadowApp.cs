using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Power;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ina260 ina260;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            var bus = Device.CreateI2cBus();
            ina260 = new Ina260(bus);

            Console.WriteLine($"-- INA260 Sample App ---");
            Console.WriteLine($"Manufacturer: {ina260.ManufacturerID}");
            Console.WriteLine($"Die: {ina260.DieID}");
            ina260.Updated += (s, v) =>
            {
                Console.WriteLine($"{v.New.Item2}V @ {v.New.Item3}A");
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        { 
            ina260.StartUpdating(TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}