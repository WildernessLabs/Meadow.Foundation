using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric.Dhtxx;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.Dht12_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        Dht12 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            sensor = new Dht12(Device.CreateI2cBus());

            var consumer = Dht12.CreateObserver(
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
            sensor.Subscribe(consumer);

            sensor.Updated += (object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity)> e) => 
            {
                Console.WriteLine($"  Temperature: {e.New.Temperature?.Celsius:N2}C");
                Console.WriteLine($"  Relative Humidity: {e.New.Humidity:N2}%");
            };

            ReadConditions().Wait();

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        async Task ReadConditions()
        {
            var conditions = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");
        }

        //<!—SNOP—>
    }
}