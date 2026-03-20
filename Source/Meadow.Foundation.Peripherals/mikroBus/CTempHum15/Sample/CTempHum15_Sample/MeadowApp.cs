using Meadow;
using Meadow.Devices;
using Meadow.Foundation.mikroBUS.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace CTempHum15_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        CTempHum15 cTempHum15;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            cTempHum15 = new CTempHum15(Device.CreateI2cBus());

            var consumer = CTempHum15.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                filter: result =>
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
                    if (result.Old is { } old)
                    {
                        return (
                        (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5
                        &&
                        (result.New.Humidity.Value.Percent - old.Humidity.Value.Percent) > 0.05
                        );
                    }
                    return false;
                }
            );
            cTempHum15.Subscribe(consumer);

            cTempHum15.Updated += (sender, result) =>
            {
                Console.WriteLine($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
                Console.WriteLine($"  Relative Humidity: {result.New.Humidity:N2}%");
            };

            ReadConditions().Wait();

            cTempHum15.StartUpdating(TimeSpan.FromSeconds(1));
        }

        async Task ReadConditions()
        {
            var conditions = await cTempHum15.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");
        }

        //<!=SNOP=>
    }
}