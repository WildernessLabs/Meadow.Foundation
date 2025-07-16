using Meadow;
using Meadow.Devices;
using Meadow.Foundation.mikroBUS;
using System;
using System.Threading.Tasks;

namespace CThermo13_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private CThermo13 _thermo;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            _thermo = new CThermo13(Device.CreateI2cBus());

            var consumer = CThermo13.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Observer: Temp changed by threshold; new temp: {result.New.Celsius:N2}C, old: {result.Old?.Celsius:N2}C");
                },
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return (result.New - old).Abs().Celsius > 0.5;
                    }
                    return false;
                }
            );
            _thermo.Subscribe(consumer);

            _thermo.Updated += (sender, result) =>
            {
                Console.WriteLine($"  Temperature: {result.New.Celsius:N2}C");
            };

            ReadConditions().Wait();

            _thermo.StartUpdating(TimeSpan.FromSeconds(1));
        }

        private async Task ReadConditions()
        {
            var conditions = await _thermo.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {conditions.Celsius:N2}C");
        }

        //<!=SNOP=>
    }
}