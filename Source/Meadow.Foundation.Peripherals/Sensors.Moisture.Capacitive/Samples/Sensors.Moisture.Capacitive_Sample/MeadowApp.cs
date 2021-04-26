using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading;
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
                analogPort: Device.CreateAnalogInputPort(Device.Pins.A01),
                minimumVoltageCalibration: 2.84f,
                maximumVoltageCalibration: 1.63f
            );

            TestCapacitiveUpdating();
            //TestCapacitiveRead();
        }

        void TestCapacitiveUpdating() 
        {
            Console.WriteLine("TestCapacitiveUpdating...");

            var consumer = Capacitive.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Moisture values: {Math.Truncate(result.New.Value)}, old: {Math.Truncate(result.Old.Value)}, delta: {result.Delta.Value}");
                },
                filter: null
            );
            capacitive.Subscribe(consumer);

            capacitive.HumidityUpdated += (object sender, CompositeChangeResult<ScalarDouble> e) =>
            {
                Console.WriteLine($"Moisture Updated: {e.New.Value}");
            };

            capacitive.StartUpdating();
        }

        async Task TestCapacitiveRead()
        {
            Console.WriteLine("TestCapacitiveSensor...");

            while (true)
            {
                var moisture = await capacitive.Read();

                Console.WriteLine($"Moisture New Value { moisture.New.Value}");
                Console.WriteLine($"Moisture Old Value { moisture.Old.Value}");
                Console.WriteLine($"Moisture Delta Value { moisture.Delta.Value}");
                Thread.Sleep(1000);
            }
        }
    }
}