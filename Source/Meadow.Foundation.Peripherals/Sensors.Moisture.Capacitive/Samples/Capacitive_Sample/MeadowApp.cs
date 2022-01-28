using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Sensors.Moisture.Capacitive_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        Capacitive capacitive;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            capacitive = new Capacitive(
                analogPort: Device.CreateAnalogInputPort(Device.Pins.A00, 5, TimeSpan.FromMilliseconds(40), new Voltage(3.3, Voltage.UnitType.Volts)),
                minimumVoltageCalibration: new Voltage(2.84f),
                maximumVoltageCalibration: new Voltage(1.63f)
            );

            // Example that uses an IObservable subscription to only be notified when the humidity changes by filter defined.
            var consumer = Capacitive.CreateObserver(
                handler: result => {
                    // the first time through, old will be null.
                    string oldValue = (result.Old is { } old) ? $"{old:n2}" : "n/a"; // C# 8 pattern matching
                    Console.WriteLine($"Subscribed - " +
                        $"new: {result.New}, " +
                        $"old: {oldValue}");
                },
                filter: null
            );
            capacitive.Subscribe(consumer);

            // classical .NET events can also be used:
            capacitive.HumidityUpdated += (sender, result) =>
            {   // the first time through, old will be null.
                string oldValue = (result.Old is { } old) ? $"{old:n2}" : "n/a"; // C# 8 pattern matching
                Console.WriteLine($"Updated - New: {result.New}, Old: {oldValue}");
            };

            // Get an initial reading.
            ReadMoisture().Wait();

            // Spin up the sampling thread so that events are raised and IObservable notifications are sent.
            capacitive.StartUpdating(TimeSpan.FromSeconds(5));
        }

        protected async Task ReadMoisture()
        {
            var moisture = await capacitive.Read();
            Console.WriteLine($"Moisture New Value { moisture }");            
        }
        //<!—SNOP—>
    }
}