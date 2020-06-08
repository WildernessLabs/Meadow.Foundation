using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location;
using Meadow.Peripherals.Sensors.Location.Gnss;
using Meadow.Foundation.Communications;

namespace Sensors.Location.MediaTek
{
    public class Mt3339
    {
        ISerialPort serialPort;

        public Mt3339(ISerialPort serialPort, int baud = 9600)
        {
            this.serialPort = serialPort;
            this.serialPort.BaudRate = baud;
            Init();
        }

        protected void Init()
        {
            Console.WriteLine("initializing serial port");
            serialPort.Open();
            Console.WriteLine("serial port opened.");
        }

        public void StartDumpingReadings()
        {
            var serialTextFile = new SerialTextFile(serialPort, "\r\n");
            serialTextFile.OnLineReceived += (s, line) => {
                Console.WriteLine(line);
            };
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
