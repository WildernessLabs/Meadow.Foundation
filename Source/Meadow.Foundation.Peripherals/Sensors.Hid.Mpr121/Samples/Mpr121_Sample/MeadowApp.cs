using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;

namespace Sensors.Distance.Mpr121_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            var sensor = new Mpr121(Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard), 90, 100);
            sensor.ChannelStatusesChanged += Sensor_ChannelStatusesChanged;

            return Task.CompletedTask;
        }

        private void Sensor_ChannelStatusesChanged(object sender, ChannelStatusChangedEventArgs e)
        {
            string pads = string.Empty;

            for(int i = 0; i < e.ChannelStatus.Count; i++)
            {
                if(e.ChannelStatus[(Mpr121.Channels)i] == true)
                {
                    pads += i + ", ";
                }
            }

            var msg = string.IsNullOrEmpty(pads) ? "none" : (pads + "touched");
            Console.WriteLine(msg);
        }

        //<!=SNOP=>
    }
}