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
        SFSR02 sFSR02;

        public MeadowApp()
        {
            sFSR02 = new SFSR02(Device, Device.Pins.D03);
            sFSR02.DistanceDetected += SFSR02DistanceDetected;

            while (true)
            {
                Console.WriteLine("Measure Distance:");

                // Sends a trigger signal
                sFSR02.MeasureDistance();
                Thread.Sleep(1500);
            }
        }

        // Prints the measured distance after sending a trigger signal
        // Valid distance ranges from 2cm to 400cm. Prints -1 otherwise.
        private void SFSR02DistanceDetected(object sender, Meadow.Peripherals.Sensors.Distance.DistanceEventArgs e)
        {
            Console.WriteLine(e.Distance.ToString());
        }
    }
}