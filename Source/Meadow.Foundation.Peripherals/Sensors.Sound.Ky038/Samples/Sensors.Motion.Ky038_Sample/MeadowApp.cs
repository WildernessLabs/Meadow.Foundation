using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Sound;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Ky038 sensor;

        public MeadowApp()
        {
            InitHardware();
        }

        public void InitHardware()
        {
            Console.WriteLine("Initialize...");

            sensor = new Ky038(Device, Device.Pins.A00, Device.Pins.D10);
        }
    }
}