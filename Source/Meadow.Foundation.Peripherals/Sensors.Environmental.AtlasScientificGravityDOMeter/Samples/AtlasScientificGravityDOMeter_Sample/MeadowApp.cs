using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using System;
using System.Threading.Tasks;

namespace Sensors.Environmental.AtlasScientificGravityDOMeter_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        AtlasScientificGravityDOMeter sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new AtlasScientificGravityDOMeter(Device.Pins.A00);

            // Example that uses an IObservable subscription to only be notified when the saturation changes by filter defined
            var consumer = AtlasScientificGravityDOMeter.CreateObserver(
                handler: result =>
                {
                    string oldValue = (result.Old is { } old) ? $"{old:n2}" : "n/a";
                    Resolver.Log.Info($"Subscribed - " +
                        $"new: {result.New}%, " +
                        $"old: {oldValue}%");
                },
                filter: null
            );
            sensor.Subscribe(consumer);

            // classical .NET events can also be used:
            sensor.SaturationUpdated += (sender, result) =>
            {
                string oldValue = (result.Old is { } old) ? $"{old:n2}" : "n/a";
                Resolver.Log.Info($"Updated - New: {result.New}%, Old: {oldValue}%");
            };

            //==== One-off reading use case/pattern
            ReadSensor().Wait();

            sensor.StartUpdating(TimeSpan.FromMilliseconds(1000));

            return Task.CompletedTask;
        }

        protected async Task ReadSensor()
        {
            var saturation = await sensor.Read();
            Resolver.Log.Info($"Initial saturation: {saturation:N2}%");
        }

        //<!=SNOP=>
    }
}