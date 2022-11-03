using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace Sensors.Distance.Mb10x0_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mb10x0 sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            sensor = new Mb10x0(Device, Device.SerialPortNames.Com4);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            while (true)
            {
                sensor.ReadSerial();

                Thread.Sleep(500);
            }
        }

        //<!=SNOP=>
    }
}
