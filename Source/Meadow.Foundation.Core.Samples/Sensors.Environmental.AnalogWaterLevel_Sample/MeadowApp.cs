using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.AnalogWaterLevel_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        AnalogWaterLevel analogWaterLevel;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            // configure our AnalogWaterLevel sensor
            analogWaterLevel = new AnalogWaterLevel(
                device: Device,
                analogPin: Device.Pins.A00
            );

            // Example that uses an IObservable subscription to only be notified
            // when the level changes by at least 0.1cm
            analogWaterLevel.Subscribe(AnalogWaterLevel.CreateObserver(
                h => Resolver.Log.Info($"Water level changed by 10 mm; new: {h.New}, old: {h.Old}"),
                null //e => { return Math.Abs(e.Delta) > 0.1f; }
            ));

            // classical .NET events can also be used:
            analogWaterLevel.Updated += (object sender, IChangeResult<float> e) => {
                Resolver.Log.Info($"Level Changed, level: {e.New}cm");
            };

            // Get an initial reading.
            ReadLevel().Wait();

            // Spin up the sampling thread so that events are raised and IObservable notifications are sent.
            analogWaterLevel.StartUpdating(TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        protected async Task ReadLevel()
        {
            var conditions = await analogWaterLevel.Read();
            Resolver.Log.Info($"Initial level: { conditions }");
        }

        //<!=SNOP=>
    }
}