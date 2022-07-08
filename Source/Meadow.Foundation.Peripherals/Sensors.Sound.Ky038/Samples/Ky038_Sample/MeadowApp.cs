using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Sound;

namespace Sensors.Sound.Ky038_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ky038 sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            sensor = new Ky038(Device, Device.Pins.A00, Device.Pins.D10);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}