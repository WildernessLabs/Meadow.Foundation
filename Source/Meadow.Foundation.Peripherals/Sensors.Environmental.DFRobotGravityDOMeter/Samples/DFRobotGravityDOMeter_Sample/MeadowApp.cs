using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using System;
using System.Threading.Tasks;

namespace Sensors.Environmental.DFRobotGravityDOMeter_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        DFRobotGravityDOMeter sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new DFRobotGravityDOMeter(Device.Pins.A01);

            // Example that uses an IObservable subscription to only be notified when the saturation changes
            var consumer = DFRobotGravityDOMeter.CreateObserver(
                handler: result =>
                {
                    string oldValue = (result.Old is { } old) ? $"{old.MilligramsPerLiter:n0}" : "n/a";
                    string newValue = $"{result.New.MilligramsPerLiter:n0}";
                    Resolver.Log.Info($"New: {newValue}mg/l, Old: {oldValue}mg/l");
                },
                filter: null
            );
            sensor.Subscribe(consumer);

            // optional classical .NET events can also be used:
            sensor.Updated += (sender, result) =>
            {
                string oldValue = (result.Old is { } old) ? $"{old.MilligramsPerLiter}mg/l" : "n/a";
                Resolver.Log.Info($"Updated - New: {result.New.MilligramsPerLiter:n0}mg/l, Old: {oldValue}");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            await ReadSensor();

            sensor.StartUpdating(TimeSpan.FromSeconds(2));
        }

        protected async Task ReadSensor()
        {
            var concentration = await sensor.Read();
            Resolver.Log.Info($"Initial concentration: {concentration.MilligramsPerLiter:N0}mg/l");
        }

        //<!=SNOP=>
    }
}