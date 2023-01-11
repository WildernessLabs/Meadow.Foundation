using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;

namespace Sensors.Environmental.Y4000_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Y4000 sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing...");

            return base.Initialize();
        }

        //<!=SNOP=>
    }
}