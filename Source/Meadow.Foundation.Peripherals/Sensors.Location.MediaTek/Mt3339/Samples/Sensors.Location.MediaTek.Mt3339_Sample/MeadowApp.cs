using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Sensors.Location.MediaTek;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mt3339 gps;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            ISerialPort serial = Device.CreateSerialPort(Device.SerialPortNames.Com4, 9600);
            gps = new Mt3339(serial);
        }

    }
}