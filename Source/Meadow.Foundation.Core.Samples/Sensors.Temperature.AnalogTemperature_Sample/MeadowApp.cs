using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Peripherals.Sensors.Atmospheric;

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
            analogTemperature.Subscribe(new FilterableObserver<AtmosphericConditionChangeResult, AtmosphericConditions>(
                h => {
                    Console.WriteLine($"Temp changed by a degree; new: {h.New.Temperature}, old: {h.Old.Temperature}");
                },
                e => {
                    return (Math.Abs(e.Delta.Temperature) > 1);
                }
                ));

            // classical .NET events can also be used:
            analogTemperature.Updated += (object sender, AtmosphericConditionChangeResult e) => {
                Console.WriteLine($"Temp Changed, temp: {e.New.Temperature}ºC");
            };

            // Get an initial reading.
            ReadTemp().Wait();

            // Spin up the sampling thread so that events are raised and
            // IObservable notifications are sent.
            analogTemperature.StartUpdating();
        }

        protected async Task ReadTemp()
        {
            var conditions = await analogTemperature.Read();
            Console.WriteLine($"Initial temp: { conditions.Temperature }");
        }
    }
}