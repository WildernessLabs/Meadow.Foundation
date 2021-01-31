using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;

namespace Sensors.Distance.Mpr121_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mpr121 sensor;

        public MeadowApp()
        {
            Init();

        }

        public void Init()
        {
            Console.WriteLine("Init...");

            sensor = new Mpr121(Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard), 90, 100);
            sensor.ChannelStatusesChanged += Sensor_ChannelStatusesChanged;
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

            if (string.IsNullOrEmpty(pads))
            {
                Console.WriteLine("none");
            }
            else
            {
                Console.WriteLine(pads + "touched");
            }
        }
    }
}