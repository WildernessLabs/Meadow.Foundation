using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Sensors.Moisture.Capacitive_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Capacitive capacitive;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            capacitive = new Capacitive(
                Device.Pins.A00,
                minimumVoltageCalibration: new Voltage(2.84f),
                maximumVoltageCalibration: new Voltage(1.63f)
            );

            // Example that uses an IObservable subscription to only be notified when the humidity changes by filter defined.
            var consumer = Capacitive.CreateObserver(
                handler: result =>
                {
                    string oldValue = (result.Old is { } old) ? $"{old:n2}" : "n/a";
                    Resolver.Log.Info($"Subscribed - " +
                        $"new: {result.New}, " +
                        $"old: {oldValue}");
                },
                filter: null
            );
            capacitive.Subscribe(consumer);

            // classical .NET events can also be used:
            capacitive.HumidityUpdated += (sender, result) =>
            {
                string oldValue = (result.Old is { } old) ? $"{old:n2}" : "n/a";
                Resolver.Log.Info($"Updated - New: {result.New}, Old: {oldValue}");
            };

            //==== One-off reading use case/pattern
            ReadSensor().Wait();

            capacitive.StartUpdating(TimeSpan.FromMilliseconds(1000));

            return Task.CompletedTask;
        }

        protected async Task ReadSensor()
        {
            var humidity = await capacitive.Read();
            Resolver.Log.Info($"Initial humidity: {humidity:n2}");
        }

        //<!=SNOP=>
    }
}