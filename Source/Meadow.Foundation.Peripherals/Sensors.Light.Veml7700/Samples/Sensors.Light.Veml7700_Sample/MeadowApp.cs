using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace Sensors.Light.Veml7700_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Veml7700 _veml;

        public MeadowApp()
        {
            var bus = Device.CreateI2cBus();
            using (_veml = new Veml7700(bus))
            {

                _veml.ChangeThreshold = 10;
    
                while (true)
                {
                    Thread.Sleep(5000);
                }
            }
        }
    }
}