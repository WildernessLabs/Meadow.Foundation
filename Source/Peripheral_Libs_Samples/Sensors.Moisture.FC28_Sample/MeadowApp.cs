using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;

namespace Sensors.Moisture.FC28_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        FC28 fc28;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            fc28 = new FC28(
                Device.CreateAnalogInputPort(Device.Pins.A01),
                Device.CreateDigitalOutputPort(Device.Pins.D15),
                minimumVoltageCalibration: 3.24f,
                maximumVoltageCalibration: 2.25f
            );
            fc28.Updated += FC28Updated;

            TestFC28Updating();
            //TestFC28Read();
        }

        void FC28Updated(object sender, FloatChangeResult e)
        {
            Console.WriteLine($"Moisture {(int)(e.New * 100)}%");
        }

        void TestFC28Updating() 
        {
            Console.WriteLine("TestFC28Updating...");

            fc28.StartUpdating();
        }

        void TestFC28Read()
        {
            Console.WriteLine("TestFC28Sensor...");

            // Use Read(); to get soil moisture value from 0 - 100
            while (true)
            {
                float moisture = fc28.Read().Result;

                if (moisture > 1.0f)
                    moisture = 1.0f;
                else
                if (moisture < 0)
                    moisture = 0;

                Console.WriteLine($"Moisture {(int)(moisture * 100)}%");
                Thread.Sleep(1000);
            }
        }
    }
}