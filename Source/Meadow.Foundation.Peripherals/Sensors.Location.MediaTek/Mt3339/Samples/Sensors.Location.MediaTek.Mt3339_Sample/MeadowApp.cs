using System;
using System.Text;
using Meadow;
using Meadow.Devices;
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
            ISerialMessagePort serial = Device.CreateSerialMessagePort(
                Device.SerialPortNames.Com4,
                suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                preserveDelimiter: true,
                baudRate: 9600);

            gps = new Mt3339(serial);
        }
    }
}