using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Htu21d htu21d;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // create the I2C Bus
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Standard);

            Console.WriteLine("Created I2C Bus");

            // create our device
            htu21d = new Htu21d(i2cBus);

            Console.WriteLine($"Chip Serial: {htu21d.SerialNumber}");

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            htu21d.StartUpdating();

            //==== Events
            // classical .NET events can also be used:
            htu21d.Updated += (object sender, IChangeResult<(Temperature Temperature, RelativeHumidity Humidity)> result) => {
                Console.WriteLine($"  Temperature: {result.New.Temperature.Celsius:F1}°C");
                Console.WriteLine($"  Relative Humidity: {result.New.Humidity.Percent:F1}%");
            };

            //==== IObservable
            var consumer = Htu21d.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer triggered; new temp: {result.New.Item1.Celsius}, old: {result.Old?.Item1.Celsius}");
                },
                filter: result => {
                    return true;
                    //return (
                    //    (Math.Abs(result.Delta.Value.Unit1.Celsius) > 1)
                    //    &&
                    //    (Math.Abs(result.Delta.Value.Unit3.Bar) > 5)
                    //    );
                });
            htu21d.Subscribe(consumer);
        }

        protected async Task ReadConditions()
        {
            var result = await htu21d.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {result.Temperature.Celsius:F1}°C");
            Console.WriteLine($"  Relative Humidity: {result.Humidity:F1}%");
        }
    }
}