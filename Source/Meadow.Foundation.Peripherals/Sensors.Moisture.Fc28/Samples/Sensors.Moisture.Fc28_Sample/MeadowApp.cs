using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using VU = Meadow.Units.Voltage.UnitType;

namespace Sensors.Moisture.FC28_Sample
{
    // TODO: this needs a better sample.
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Fc28 fc28;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            fc28 = new Fc28(
                Device.CreateAnalogInputPort(Device.Pins.A01),
                Device.CreateDigitalOutputPort(Device.Pins.D15),
                minimumVoltageCalibration: new Voltage(3.24f, VU.Volts),
                maximumVoltageCalibration: new Voltage(2.25f, VU.Volts)
            );

            TestFC28Updating();
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

            fc28.HumidityUpdated += (object sender, IChangeResult<double> e) =>
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

                Console.WriteLine($"Moisture Value { moisture}");
                Thread.Sleep(1000);
            }
        }
    }
}