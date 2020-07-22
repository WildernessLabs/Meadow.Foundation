using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Hid;

namespace MeadowApp
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
            Console.WriteLine("Touched");
        }
    }
}