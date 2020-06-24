using System;
using System.Text;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GPSWing gps;

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

            gps = new GPSWing(serial);
        }
    }
}