using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using System;
using System.Threading;

namespace HYSRF05_Sample
{
    public class HYSRF05App : App<F7Micro, HYSRF05App>
    {
        public HYSRF05App()
        {
            var HYSRF05 = new HYSRF05(Device, Device.Pins.D05, Device.Pins.D06);
            HYSRF05.DistanceDetected += HYSRF05DistanceDetected; ;

            while (true)
            {
                // Sends a trigger signal
                HYSRF05.MeasureDistance();
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
