using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;

namespace ICs.IOExpanders.Ds3502_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        public MeadowApp()
        {
            Console.WriteLine("Initialize...");
        }

        //<!=SNOP=>
    }
}