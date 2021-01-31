using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.Mb10x0_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mb10x0 sensor;

        public MeadowApp()
        {
            Initialize();

            while (true)
            {
                sensor.ReadSerial();

                Thread.Sleep(500);
            }
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            sensor = new Mb10x0(Device, Device.SerialPortNames.Com4);
        }
    }
}
