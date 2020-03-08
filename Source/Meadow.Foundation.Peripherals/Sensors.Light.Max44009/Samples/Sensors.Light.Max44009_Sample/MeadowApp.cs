using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace Sensors.Light.Max44009_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Max44009 sensor;

        public MeadowApp()
        {
            var bus = Device.CreateI2cBus();
            sensor = new Max44009(bus);

            while (true)
            {
                Thread.Sleep(1000);

                Console.WriteLine($"Lux: {sensor.GetIlluminance()}");
            }
        }
    }
}