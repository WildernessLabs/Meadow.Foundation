using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace Sensors.Light.Veml7700_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        VEML7700 _veml;

        public MeadowApp()
        {
            var bus = Device.CreateI2cBus();
            using (_veml = new VEML7700(bus))
            {

                _veml.ChangeThreshold = 10;
                _veml.LuxChanged += OnLightChanged;

                while (true)
                {
                    Thread.Sleep(5000);
                }
            }
        }

        private void OnLightChanged(float previousValue, float newValue)
        {
            Console.WriteLine($"Light: {_veml.Lux} lux");
        }
    }
}