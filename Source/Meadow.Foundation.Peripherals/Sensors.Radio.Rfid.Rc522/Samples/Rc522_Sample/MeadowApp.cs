using System;
using Meadow;
using Meadow.Devices;

namespace Meadow.Foundation.Sensors.Rfid_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public MeadowApp()
        {
             Console.WriteLine("Initialize hardware...");
        }
    }
}