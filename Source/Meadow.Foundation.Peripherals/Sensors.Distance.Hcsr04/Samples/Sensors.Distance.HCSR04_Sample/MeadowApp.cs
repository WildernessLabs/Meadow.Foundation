using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.HCSR04_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        HCSR04 hCSR04;

        public MeadowApp()
        {
            hCSR04 = new HCSR04(Device, Device.Pins.D05, Device.Pins.D06);
            hCSR04.DistanceDetected += HCSR04DistanceDetected;

            while (true)
            {
                // Sends a trigger signal
                hCSR04.MeasureDistance();
                Thread.Sleep(1500);
            }
        }

        // Prints the measured distance after sending a trigger signal
        // Valid distance ranges from 2cm to 400cm. Prints -1 otherwise.
        private void HCSR04DistanceDetected(object sender, Meadow.Peripherals.Sensors.Distance.DistanceEventArgs e)
        {
            Console.WriteLine(e.Distance.ToString());
        }
    }
}