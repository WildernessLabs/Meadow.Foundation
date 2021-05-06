using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Sensors.Moisture.Capacitive_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Capacitive capacitive;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            capacitive = new Capacitive(
                analogPort: Device.CreateAnalogInputPort(Device.Pins.A00),
                minimumVoltageCalibration: new Voltage(2.84f),
                maximumVoltageCalibration: new Voltage(1.63f)
            );

            //==== IObservable Pattern
            // Example that uses an IObservable subscription to only be notified
            // when the humidity changes by filter defined.
            var consumer = Capacitive.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Subscribed - " +
                        $"new: {result.New}, " +
                        $"old: {result.Old.Value}, " +
                        $"delta: {result.Delta.Value}");
                },
                filter: null
            );
            capacitive.Subscribe(consumer);

            //==== Classic Events
            // classical .NET events can also be used:
            capacitive.HumidityUpdated += (object sender, ChangeResult<double> e) =>
            {
                Console.WriteLine($"Updated - New: {e.New}, Old: {e.Old.Value}, Delta: {e.Delta.Value}");
            };

            // Get an initial reading.
            ReadMoisture().Wait();

            // Spin up the sampling thread so that events are raised and
            // IObservable notifications are sent.
            capacitive.StartUpdating();
        }

        protected async Task ReadMoisture()
        {
            var moisture = await capacitive.Read();
            Console.WriteLine($"Moisture New Value { moisture.New}");            
        }
    }
}