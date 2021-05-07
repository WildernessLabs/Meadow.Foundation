using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.SFSR02_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Sfsr02 sFSR02;

        public MeadowApp()
        {
            sFSR02 = new Sfsr02(Device, Device.Pins.D03);
            sFSR02.DistanceUpdated += SFSR02_DistanceUpdated;

            while (true)
            {
                Console.WriteLine("Measure Distance:");

                // Sends a trigger signal
                sFSR02.MeasureDistance();
                Thread.Sleep(1500);
            }
        }

        private void SFSR02_DistanceUpdated(object sender, IChangeResult<Meadow.Units.Length> e)
        {
            Console.WriteLine($"{e.New.Centimeters}cm");
        }
    }
}