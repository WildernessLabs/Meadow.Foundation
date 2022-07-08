using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Si1145 sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            sensor = new Si1145(Device.CreateI2cBus());

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Si1145.CreateObserver(
                handler: result => Console.WriteLine($"Observer: filter satisifed: {result.New.VisibleLight?.Lux:N2}Lux, old: {result.Old?.VisibleLight?.Lux:N2}Lux"),
           
                // only notify if the visible light changes by 100 lux (put your hand over the sensor to trigger)
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        // returns true if > 100lux change
                        return ((result.New.VisibleLight.Value - old.VisibleLight.Value).Abs().Lux > 100);
                    }
                    return false;
                });

            sensor.Subscribe(consumer);

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Console.WriteLine($" Visible Light: {result.New.VisibleLight?.Lux:N2}Lux");
                Console.WriteLine($" Infrared Light: {result.New.Infrared?.Lux:N2}Lux");
                Console.WriteLine($" UV Index: {result.New.UltravioletIndex:N2}Lux");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var (VisibleLight, UltravioletIndex, Infrared) = await sensor.Read();

            Console.WriteLine("Initial Readings:");
            Console.WriteLine($" Visible Light: {VisibleLight?.Lux:N2}Lux");
            Console.WriteLine($" Infrared Light: {Infrared?.Lux:N2}Lux");
            Console.WriteLine($" UV Index: {UltravioletIndex:N2}Lux");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}