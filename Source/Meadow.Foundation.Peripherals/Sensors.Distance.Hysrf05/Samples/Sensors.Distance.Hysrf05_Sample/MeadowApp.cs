using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.HYSRF05_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Hysrf05 hYSRF05;

        public MeadowApp()
        {
            hYSRF05 = new Hysrf05(Device, Device.Pins.D05, Device.Pins.D06);
            hYSRF05.DistanceUpdated += HYSRF05_DistanceUpdated;

            while (true)
            {
                // Sends a trigger signal
                hYSRF05.MeasureDistance();
                Thread.Sleep(500);
            }
        }

        private void HYSRF05_DistanceUpdated(object sender, IChangeResult<Meadow.Units.Length> e)
        {
            Console.WriteLine($"Distance is {e.New.Centimeters}cm");
        }
    }
}