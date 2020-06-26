using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace Sensors.Light.Tsl2591_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Tsl2591 tsl;

        public MeadowApp()
        {
            var bus = Device.CreateI2cBus();
            tsl = new Tsl2591(bus);

            tsl.PowerOn();
            tsl.ChangeThreshold = 10;
            tsl.Channel0Changed += OnLightChange;
            tsl.StartSampling(TimeSpan.FromSeconds(1));

            while (true)
            {
                Thread.Sleep(5000);
            }
        }

        void OnLightChange(int before, int after)
        {
            Console.WriteLine($"Light: 0:{tsl.Channel0}");
        }
    }
}