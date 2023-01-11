using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.HCSR04_Sample
{
    // Driver in development - not currently working
    // Meadow is currently not reading timing deltas smaller than 10,000 ticks (10ms) in b3.7
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Hcsr04 hCSR04;

        public override Task Initialize()
        {
            Resolver.Log.Info($"Hello HC-SR04 sample");

            hCSR04 = new Hcsr04(
                device: Device, 
                triggerPin: Device.Pins.D05, 
                echoPin: Device.Pins.D06);
            hCSR04.DistanceUpdated += HCSR04_DistanceUpdated;

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            while (true)
            {
                // Sends a trigger signal
                hCSR04.MeasureDistance();
                Thread.Sleep(2000);
            }
        }

        private void HCSR04_DistanceUpdated(object sender, IChangeResult<Meadow.Units.Length> e)
        {
            Resolver.Log.Info($"Distance (cm): {e.New.Centimeters}");
        }

        //<!=SNOP=>
    }
}