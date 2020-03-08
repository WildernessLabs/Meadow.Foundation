using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace Sensors.Light.Tsl2591_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        TSL2591 _tsl;

        public MeadowApp()
        {
            var bus = Device.CreateI2cBus();
            _tsl = new TSL2591(bus);

            _tsl.PowerOn();
            _tsl.ChangeThreshold = 10;
            _tsl.Channel0Changed += OnLightChange;
            _tsl.StartSampling(TimeSpan.FromSeconds(1));

            while (true)
            {
                Thread.Sleep(5000);
            }
        }

        void OnLightChange(int before, int after)
        {
            Console.WriteLine($"Light: 0:{_tsl.Channel0}");
        }
    }
}