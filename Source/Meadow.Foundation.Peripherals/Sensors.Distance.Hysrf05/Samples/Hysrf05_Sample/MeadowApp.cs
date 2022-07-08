using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.HYSRF05_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Hysrf05 hYSRF05;

        public override Task Initialize()
        {
            hYSRF05 = new Hysrf05(
                device: Device, 
                triggerPin: Device.Pins.D05, 
                echoPin: Device.Pins.D06);
            hYSRF05.DistanceUpdated += HYSRF05_DistanceUpdated;

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            while (true)
            {
                // Sends a trigger signal
                hYSRF05.MeasureDistance();
                Thread.Sleep(500);
            }

            return Task.CompletedTask;
        }

        private void HYSRF05_DistanceUpdated(object sender, IChangeResult<Meadow.Units.Length> e)
        {
            Console.WriteLine($"Distance is {e.New.Centimeters}cm");
        }

        //<!=SNOP=>
    }
}