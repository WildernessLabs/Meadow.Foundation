using System;
using System.Text;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.GPS;
using Meadow.Hardware;
using Sensors.Location.MediaTek;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mt3339 gps;

        public MeadowApp()
        {
            Console.WriteLine("App Start");

            Initialize();

            Console.WriteLine("Post Init");

            gps.StartUpdataing();
        }

        void Initialize()
        {
            // TODO: need to make an overload that takes a device and a serial
            // port name, so that we can ensure these things are configured
            // correctly
            SerialMessagePort serial = Device.CreateSerialMessagePort(
                Device.SerialPortNames.Com4,
                suffixDelimiter: Encoding.ASCII.GetBytes("\n"),
                preserveDelimiter: true,
                9600
                );
            gps = new Mt3339(serial);
        }
    }
}