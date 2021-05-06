using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sensors.Moisture.FC28_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Fc28 fc28;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            fc28 = new Fc28(
                Device.CreateAnalogInputPort(Device.Pins.A01),
                Device.CreateDigitalOutputPort(Device.Pins.D15),
                minimumVoltageCalibration: 3.24f,
                maximumVoltageCalibration: 2.25f
            );

            TestFC28Updating();
            //TestFC28Read();
        }

        void TestFC28Updating() 
        {
            Console.WriteLine("TestFC28Updating...");

            var consumer = Fc28.CreateObserver(
                handler: result => 
                { 
                
                }, 
                filter: null
            );
            fc28.Subscribe(consumer);

            fc28.HumidityUpdated += (object sender, ChangeResult<double> e) =>
            {
                Console.WriteLine($"Moisture Updated: {e.New}");
            };

            fc28.StartUpdating();
        }

        async Task TestFC28Read()
        {
            Console.WriteLine("TestFC28Sensor...");

            while (true)
            {
                var moisture = await fc28.Read();

                Console.WriteLine($"Moisture New Value { moisture.New}");
                Console.WriteLine($"Moisture Old Value { moisture.Old.Value}");
                Console.WriteLine($"Moisture Delta Value { moisture.Delta.Value}");
                Thread.Sleep(1000);
            }
        }
    }
}