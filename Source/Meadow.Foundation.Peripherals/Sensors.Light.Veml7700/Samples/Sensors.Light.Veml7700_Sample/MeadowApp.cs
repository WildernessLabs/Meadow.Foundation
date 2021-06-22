using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;
using Meadow.Units;

namespace Sensors.Light.Veml7700_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Veml7700 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            sensor = new Veml7700(i2c);

            Console.WriteLine("Hardware initialized.");

            ////==== IObservable 
            //// Example that uses an IObersvable subscription to only be notified
            //// when the temperature changes by at least a degree, and humidty by 5%.
            //// (blowing hot breath on the sensor should trigger)
            //var consumer = Veml7700.CreateObserver(
            //    handler: result => {
            //        Console.WriteLine($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
            //    },
            //    // only notify if the change is greater than 0.5°C
            //    filter: result => {
            //        if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
            //            return (
            //            (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5 // returns true if > 0.5°C change.
            //            &&
            //            (result.New.Humidity.Value - old.Humidity.Value).Percent > 0.05 // 5% humidity change
            //            ); // returns true if > 0.5°C change.
            //        }
            //        return false;
            //    }
            //    // if you want to always get notified, pass null for the filter:
            //    //filter: null
            //    );
            //sensor.Subscribe(consumer);

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Console.WriteLine($"Illuminance: {result.New.Lux:n3}Lux");
            };

            //==== one-off read
            ReadConditions().Wait();

            // start updating continuously
            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        protected async Task ReadConditions()
        {
            var conditions = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Illuminance: {conditions.Lux:n3}Lux");
        }
    }
}