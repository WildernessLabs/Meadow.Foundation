using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location;

namespace Sensors.Location.MediaTek
{
    public class Mt3339
    {
        ISerialPort serialPort;

        public Mt3339(ISerialPort serialPort, int baud = 9600)
        {
            this.serialPort = serialPort;
            this.serialPort.BaudRate = baud;
        }

        protected void Init()
        {
            this.serialPort.Open();

        }

        public async Task<LocationInfo> Read()
        {
            var loc = new LocationInfo();
            return loc;
        }

        //public async Task<> StartUpdating()
        //{
        //}

        //public void StopUpdating()
        //{
        //}
    }
}
