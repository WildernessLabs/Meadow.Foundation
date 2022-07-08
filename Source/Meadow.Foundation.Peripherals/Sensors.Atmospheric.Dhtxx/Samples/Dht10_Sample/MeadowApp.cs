using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric.Dhtxx;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Dht10_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        Dht10 dht10;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            dht10 = new Dht10(Device.CreateI2cBus());

            var consumer = Dht10.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                filter: result =>
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
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
            dht10.Subscribe(consumer);

            dht10.Updated += (object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity)> e) =>
            {
                Console.WriteLine($"  Temperature: {e.New.Temperature?.Celsius:N2}C");
                Console.WriteLine($"  Relative Humidity: {e.New.Humidity:N2}%");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var conditions = await dht10.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");

            dht10.StartUpdating(TimeSpan.FromSeconds(1));
        }
    }
}