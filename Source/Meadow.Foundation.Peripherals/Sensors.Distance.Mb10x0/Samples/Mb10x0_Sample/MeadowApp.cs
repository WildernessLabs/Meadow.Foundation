using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.Mb10x0_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        Mb10x0 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            sensor = new Mb10x0(Device, Device.SerialPortNames.Com4);

            while (true)
            {
                sensor.ReadSerial();

                Thread.Sleep(500);
            }
        }

        //<!=SNOP=>
    }
}
