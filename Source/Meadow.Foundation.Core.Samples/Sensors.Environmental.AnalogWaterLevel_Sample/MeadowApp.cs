using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;

namespace Sensors.Temperature.AnalogWaterLevel_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        AnalogWaterLevel analogWaterLevel;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our AnalogWaterLevel sensor
            analogWaterLevel = new AnalogWaterLevel(
                device: Device,
                analogPin: Device.Pins.A00
            );

            // Example that uses an IObersvable subscription to only be notified
            // when the level changes by at least 0.1cm
            analogWaterLevel.Subscribe(AnalogWaterLevel.CreateObserver(
                h => Console.WriteLine($"Water level changed by 10 mm; new: {h.New}, old: {h.Old}"),
                // TODO: revisit this
                null //e => { return Math.Abs(e.Delta) > 0.1f; }
            ));

            // classical .NET events can also be used:
            analogWaterLevel.Updated += (object sender, IChangeResult<float> e) => {
                Console.WriteLine($"Level Changed, level: {e.New}cm");
            };

            // Get an initial reading.
            ReadLevel().Wait();

            // Spin up the sampling thread so that events are raised and
            // IObservable notifications are sent.
            analogWaterLevel.StartUpdating(TimeSpan.FromSeconds(5));
        }

        protected async Task ReadLevel()
        {
            var conditions = await analogWaterLevel.Read();
            Console.WriteLine($"Initial level: { conditions }");
        }
    }
}