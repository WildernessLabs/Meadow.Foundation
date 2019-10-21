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

            fc28 = new FC28(Device.CreateAnalogInputPort(Device.Pins.A01),
                Device.CreateDigitalOutputPort(Device.Pins.D15));

            TestFC28Sensor();
        }

        void TestFC28Sensor()
        {
            Console.WriteLine("TestFC28Sensor...");

            // Use ReadRaw(); to get dry and moist values
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(fc28.ReadRaw());
                Thread.Sleep(1000);
            }

            // Update boundary values when determined
            fc28.MinimumMoisture = 3.24f; // On open air
            fc28.MaximumMoisture = 2.25f; // Dipped in water

            // Use Read(); to get soil moisture value from 0 - 100
            while (true)
            {
                int moisture = (int)fc28.Read();

                if (moisture > 100)
                    moisture = 100;
                else
                if (moisture < 0)
                    moisture = 0;

                Console.WriteLine($"Raw: {fc28.Moisture} | Moisture {moisture}%");
                Thread.Sleep(1000);
            }
        }
    }
}