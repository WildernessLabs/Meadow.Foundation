using Meadow;
using Meadow.Devices;
using Sensors.Distance.SFSR02;
using System;
using System.Threading;

namespace SFSR02_Sample
{
    public class SFSR02App : App<F7Micro, SFSR02App>
    {
        public SFSR02App()
        {
            var SFSR02 = new SFSR02(Device, Device.Pins.D03);
            SFSR02.DistanceDetected += SFSR02DistanceDetected;

            while (true)
            {
                Console.WriteLine("Measure Distance:");

                // Sends a trigger signal
                SFSR02.MeasureDistance();
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