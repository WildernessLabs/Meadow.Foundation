using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Sound;

namespace Sensors.Sound.Ky038_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        public MeadowApp()
        {
            Console.WriteLine("Initialize...");

            var sensor = new Ky038(Device, Device.Pins.A00, Device.Pins.D10);
        }
    }
}