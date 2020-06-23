using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.Light;
using System;
using System.Threading;

namespace Sensors.LoadCell.Hx711_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Hx711 _loadSensor;

        public MeadowApp()
        {
            Console.WriteLine($"Creating Sensor...");
            using (_loadSensor = new Hx711(Device, Device.Pins.D04, Device.Pins.D03))
            {
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}