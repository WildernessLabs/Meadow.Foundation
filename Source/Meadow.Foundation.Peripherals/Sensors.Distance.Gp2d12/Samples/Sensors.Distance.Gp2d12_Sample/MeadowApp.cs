using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Gp2d12 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our  sensor
            sensor = new Gp2d12(Device, Device.Pins.A03);

            //==== IObservable Pattern with an optional notification filter.
            // Example that uses an IObersvable subscription to only be notified
            // when the filter is satisfied
            var consumer = Gp2d12.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer filter satisfied: {result.New.Centimeters:N2}cm, old: {result.Old?.Centimeters:N2}cm");
                },
                // only notify if the change is greater than 5cm
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return (result.New - old).Abs().Centimeters > 5; // returns true if > 5cm change.
                    }
                    return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
            );
            sensor.Subscribe(consumer);

            //==== Classic Events Pattern
            // classical .NET events can also be used:
            sensor.DistanceUpdated += (sender, result) => {
                Console.WriteLine($"Temp Changed, temp: {result.New.Centimeters:N2}cm, old: {result.Old?.Centimeters:N2}cm");
            };

            //==== One-off reading use case/pattern
            ReadSensor().Wait();

            // Spin up the sampling thread so that events are raised and
            // IObservable notifications are sent.
            sensor.StartUpdating(TimeSpan.FromMilliseconds(1000));
        }

        protected async Task ReadSensor()
        {
            var temperature = await sensor.Read();
            Console.WriteLine($"Initial temp: {temperature.Centimeters:N2}cm");
        }
    }
}