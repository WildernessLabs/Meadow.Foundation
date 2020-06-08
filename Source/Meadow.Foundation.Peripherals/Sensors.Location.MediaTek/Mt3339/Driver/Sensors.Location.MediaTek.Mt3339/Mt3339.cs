using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location;
using Meadow.Peripherals.Sensors.Location.Gnss;

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

        public async Task<GnssPositionInfo> Read()
        {
            var loc = new GnssPositionInfo();
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
