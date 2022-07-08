using System;
using System.Threading.Tasks;
using Meadow.Devices;

namespace Meadow.Foundation.Sensors.Rfid_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            return Task.CompletedTask;
        }
    }
}