using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;

namespace Sensors.Atmospheric.BME680_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Bme680 bme680;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            bme680 = new Bme680(Device.CreateI2cBus());
        }
    }
}