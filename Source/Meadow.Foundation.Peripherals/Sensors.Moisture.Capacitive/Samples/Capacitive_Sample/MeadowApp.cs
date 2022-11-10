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
            Console.WriteLine("Initialize...");

            capacitive = new Capacitive(
                analogInputPort: Device.CreateAnalogInputPort(Device.Pins.A00, 5, TimeSpan.FromMilliseconds(40), new Voltage(3.3, Voltage.UnitType.Volts)),
                minimumVoltageCalibration: new Voltage(2.84f),
                maximumVoltageCalibration: new Voltage(1.63f)
            );

            // Example that uses an IObservable subscription to only be notified when the humidity changes by filter defined.
            var consumer = Capacitive.CreateObserver(
                handler: result => {
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
            {   
                string oldValue = (result.Old is { } old) ? $"{old:n2}" : "n/a"; // C# 8 pattern matching
                Console.WriteLine($"Updated - New: {result.New}, Old: {oldValue}");
            };

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            var moisture = await capacitive.Read();
            Console.WriteLine($"Moisture New Value {moisture}");

            capacitive.StartUpdating(TimeSpan.FromSeconds(3));
        }
        //<!=SNOP=>
    }
}