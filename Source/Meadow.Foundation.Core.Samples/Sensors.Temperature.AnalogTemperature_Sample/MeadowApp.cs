using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;

namespace Sensors.Temperature.AnalogTemperature_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        AnalogTemperature analogTemperature;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our AnalogTemperature sensor
            analogTemperature = new AnalogTemperature (
                device: Device,
                analogPin: Device.Pins.A00,
                sensorType: AnalogTemperature.KnownSensorType.LM35
            );

            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree.
            analogTemperature.Subscribe(new FilterableObserver<FloatChangeResult, float>(
                h => {
                    Console.WriteLine($"Temp changed by a degree; new: {h.New}, old: {h.Old}");
                },
                e => {
                    return (Math.Abs(e.Delta) > 1);
                }
                ));

            // classical .NET events can also be used:
            analogTemperature.Updated += (object sender, FloatChangeResult e) => {
                Console.WriteLine($"Temp Changed, temp: {e.New}ºC");
            };

            // Get an initial reading.
            ReadTemp().Wait();

            // Spin up the sampling thread so that events are raised and
            // IObservable notifications are sent.
            analogTemperature.StartUpdating();
        }

        protected async Task ReadTemp()
        {
            var temp = await analogTemperature.ReadTemperature();
            Console.WriteLine($"Initial temp: { temp }");
        }
    }
}