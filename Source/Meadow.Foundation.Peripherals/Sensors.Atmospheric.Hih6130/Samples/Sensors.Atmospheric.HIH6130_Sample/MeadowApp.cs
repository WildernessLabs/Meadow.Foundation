using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Hih6130 sensor;

        public MeadowApp()
        {
            Initialize();

            // do a one off read
            ReadConditions().Wait();

            // start updating for periodic updates
            sensor.StartUpdating();
        }

        public void Initialize()
        {
            Console.WriteLine("Initializing hardware...");

            // create the sensor
            sensor = new Hih6130(Device.CreateI2cBus());

            // classic .NET
            sensor.Updated += (object sender, CompositeChangeResult <Temperature, RelativeHumidity> result) => {
                Console.WriteLine($"Temperature: {result.New?.Unit1.Celsius:N1}C, Humidity: {result.New?.Unit2:N1}%.");
            };

            // IObservable
            sensor.Subscribe(Hih6130.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer triggered; Temperature: {result.New?.Unit1.Celsius:N1}C, Humidity: {result.New?.Unit2:N1}%.");
                },
                filter: null
                ));

            Console.WriteLine("Hardware initialization complete.");
        }

        protected async Task ReadConditions()
        {
            var result = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {result.Temperature.Celsius:F1}C");
            Console.WriteLine($"  Relative Humidity: {result.Humidity:F1}%");
        }
    }
}