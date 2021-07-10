using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Units;

namespace BasicSensors.Atmospheric.SI7021_Sample
{
    public class MeadowApp
        :App<F7Micro, MeadowApp>
    {
        Si70xx sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our sensor on the I2C Bus
            var i2cBus = Device.CreateI2cBus();
            sensor = new Si70xx(i2cBus);

            Console.WriteLine($"Chip Serial: {sensor.SerialNumber}");

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Console.WriteLine($"  Temperature: {result.New.Temperature?.Celsius:F1}C");
                Console.WriteLine($"  Relative Humidity: {result.New.Humidity:F1}%");
            };

            //==== IObservable 
            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree, and humidty by 5%.
            // (blowing hot breath on the sensor should trigger)
            var consumer = Si70xx.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                // only notify if the change is greater than 0.5°C
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return (
                        (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5 // returns true if > 0.5°C change.
                        &&
                        (result.New.Humidity.Value - old.Humidity.Value).Percent > 0.05 // 5% humidity change
                        ); 
                    }
                    return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
                );
            sensor.Subscribe(consumer);

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        protected async Task ReadConditions()
        {
            var result = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {result.Temperature?.Celsius:F1}C");
            Console.WriteLine($"  Relative Humidity: {result.Humidity:F1}%");
        }
    }
}