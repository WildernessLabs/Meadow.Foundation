using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.HCSR04_Sample
{
    // Driver in development - not currently working
    // Meadow is currently not reading timing deltas smaller than 10,000 ticks (10ms) in b3.7
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Hcsr04 hCSR04;

        public MeadowApp()
        {
             Console.WriteLine($"Hello HC-SR04 sample");

            hCSR04 = new Hcsr04(Device, Device.Pins.D05, Device.Pins.D06);
            hCSR04.DistanceUpdated += HCSR04_DistanceUpdated;

            Console.WriteLine("Starting loop");

            while (true)
            {
                // Sends a trigger signal
                hCSR04.MeasureDistance();
                Thread.Sleep(2000);
            }
        }

        private void HCSR04_DistanceUpdated(object sender, IChangeResult<Meadow.Units.Length> e)
        {
            Console.WriteLine($"Distance (cm): {e.New.Centimeters}");
        }
    }
}