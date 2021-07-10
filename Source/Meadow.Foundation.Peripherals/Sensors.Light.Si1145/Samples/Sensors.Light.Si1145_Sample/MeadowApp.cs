using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Si1145 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            sensor = new Si1145(i2c);

            //==== IObservable 
            // Example that uses an IObersvable subscription to only be notified
            // when the filter is satisfied
            var consumer = Si1145.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer: filter satisifed: {result.New.VisibleLight?.Lux:N2}Lux, old: {result.Old?.VisibleLight?.Lux:N2}Lux");
                },
                // only notify if the visible light changes by 100 lux (put your hand over the sensor to trigger)
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        // returns true if > 100lux change
                        return ((result.New.VisibleLight.Value - old.VisibleLight.Value).Abs().Lux > 100);
                    }
                    return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
                );
            sensor.Subscribe(consumer);

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Console.WriteLine($"  Visible Light: {result.New.VisibleLight?.Lux:N2}Lux");
                Console.WriteLine($"  Infrared Light: {result.New.Infrared?.Lux:N2}Lux");
                Console.WriteLine($"  UV Index: {result.New.UltravioletIndex:N2}Lux");
            };

            //==== one-off read
            ReadConditions().Wait();

            // start updating continuously
            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        protected async Task ReadConditions()
        {
            var result = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Visible Light: {result.VisibleLight?.Lux:N2}Lux");
            Console.WriteLine($"  Infrared Light: {result.Infrared?.Lux:N2}Lux");
            Console.WriteLine($"  UV Index: {result.UltravioletIndex:N2}Lux");
        }
    }
}