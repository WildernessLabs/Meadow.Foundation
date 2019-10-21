using System;
using System.Threading;
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

            capacitive = new Capacitive(Device.CreateAnalogInputPort(Device.Pins.A01));

            TestCapacitiveSensorAsync();
        }

        void TestCapacitiveSensorAsync()
        {
            Console.WriteLine("TestCapacitiveSensorAsync...");

            // Use ReadRaw(); to get dry and moist values
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(capacitive.ReadRaw());
                Thread.Sleep(1000);
            }

            // Update boundary values when determined
            capacitive.MinimumMoisture = 2.84f; // On open air
            capacitive.MaximumMoisture = 1.37f; // Dipped in water

            // Use Read(); to get soil moisture value from 0 - 100
            while (true)
            {
                int moisture = (int) capacitive.Read();

                if (moisture > 100)
                    moisture = 100;
                else 
                if (moisture < 0)
                    moisture = 0;

                Console.WriteLine($"Raw: {capacitive.Moisture} | Moisture {moisture}%");
                Thread.Sleep(1000);
            }
        }
    }
}