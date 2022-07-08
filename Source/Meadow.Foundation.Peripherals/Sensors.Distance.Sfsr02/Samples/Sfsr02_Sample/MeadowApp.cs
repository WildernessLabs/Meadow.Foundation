using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.SFSR02_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Sfsr02 sFSR02;

        public override Task Initialize()
        {
            sFSR02 = new Sfsr02(Device, Device.Pins.D03);
            sFSR02.DistanceUpdated += SFSR02_DistanceUpdated;

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            while (true)
            {
                Console.WriteLine("Measure Distance:");

                sFSR02.MeasureDistance();
                await Task.Delay(1500);
            }
        }

        private void SFSR02_DistanceUpdated(object sender, IChangeResult<Meadow.Units.Length> e)
        {
            Console.WriteLine($"{e.New.Centimeters}cm");
        }

        //<!=SNOP=>
    }
}