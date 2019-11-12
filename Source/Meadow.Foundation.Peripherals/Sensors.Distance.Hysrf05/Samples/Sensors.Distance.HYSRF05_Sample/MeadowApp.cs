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
        HYSRF05 hYSRF05;

        public MeadowApp()
        {
            hYSRF05 = new HYSRF05(Device, Device.Pins.D05, Device.Pins.D06);
            hYSRF05.DistanceDetected += HYSRF05DistanceDetected;

            while (true)
            {
                // Sends a trigger signal
                hYSRF05.MeasureDistance();
                Thread.Sleep(500);
            }
        }

        // Prints the measured distance after sending a trigger signal
        // Valid distance ranges from 2cm to 400cm. Prints -1 otherwise.
        private void HYSRF05DistanceDetected(object sender, Meadow.Peripherals.Sensors.Distance.DistanceEventArgs e)
        {
            Console.WriteLine(e.Distance.ToString());
        }
    }
}
