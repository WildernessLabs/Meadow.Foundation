using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;

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

            capacitive.Subscribe(new FilterableChangeObserver<FloatChangeResult, float>(
                h => {
                    Console.WriteLine($"Moisture values: {Math.Truncate(h.New)}, old: {Math.Truncate(h.Old)}, delta: {h.DeltaPercent}");
                },
                e => {
                    return true;
                }
            ));

            capacitive.Updated += (object sender, FloatChangeResult e) =>
            {
                Console.WriteLine($"Moisture Updated: {e.New}");
            };

            capacitive.StartUpdating();
        }

        async Task TestCapacitiveRead()
        {
            Console.WriteLine("TestCapacitiveSensor...");

            while (true)
            {
                FloatChangeResult moisture = await capacitive.Read();

                Console.WriteLine($"Moisture New Value { moisture.New}");
                Console.WriteLine($"Moisture Old Value { moisture.Old}");
                Console.WriteLine($"Moisture Delta Value { moisture.Delta}");
                Thread.Sleep(1000);
            }
        }
    }
}