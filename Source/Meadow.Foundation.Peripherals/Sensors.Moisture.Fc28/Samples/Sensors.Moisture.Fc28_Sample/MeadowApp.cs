using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using Meadow.Units;

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

            fc28.HumidityUpdated += (object sender, CompositeChangeResult<ScalarDouble> e) =>
            {
                Console.WriteLine($"Moisture Updated: {e.New}");
            };

            fc28.StartUpdating();
        }

        async Task TestFC28Read()
        {
            Console.WriteLine("TestFC28Sensor...");

            // Use Read(); to get soil moisture value from 0 - 100
            while (true)
            {
                var moisture = await fc28.Read();

                Console.WriteLine($"Moisture {(int)(moisture.New.Value * 100)}%");
                Thread.Sleep(1000);
            }
        }
    }
}