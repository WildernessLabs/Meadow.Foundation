using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using System;
using System.Threading;

namespace HCSR04_Sample
{
    public class HCSR04App : App<F7Micro, HCSR04App>
    {
        public HCSR04App()
        {
            var HCSR04 = new HCSR04(Device, Device.Pins.D05, Device.Pins.D06);
            HCSR04.DistanceDetected += HCSR04DistanceDetected;

            while (true)
            {
                // Sends a trigger signal
                HCSR04.MeasureDistance();
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