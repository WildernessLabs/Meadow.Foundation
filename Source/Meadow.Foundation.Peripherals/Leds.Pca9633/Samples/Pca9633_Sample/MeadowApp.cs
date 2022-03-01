using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using System;
using System.Threading;

namespace Leds.Pca9633_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>
        Pca9633 driver;

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");
            
        }

        //<!—SNOP—>
    }
}