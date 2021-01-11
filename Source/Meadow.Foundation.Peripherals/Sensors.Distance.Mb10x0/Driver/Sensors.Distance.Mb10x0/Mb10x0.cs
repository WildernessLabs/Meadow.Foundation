using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Distance
{
    public class Mb10x0
    {
        ISerialPort serialPort;

        public int Baud => 9600;

        public Mb10x0(IIODevice device, SerialPortName portName)
        {
            serialPort = device.CreateSerialPort(portName, Baud);
            serialPort.Open();
        }

        public Mb10x0(ISerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        public int ReadSerial()
        {
            var len = serialPort.BytesToRead;

            if(len == 0) 
            {
                Console.WriteLine("No data");
                return 0; 
            }

            var data = new byte[len];

            serialPort.Read(data, 0, len);

            for(int i = 0; i <data.Length - 3; i++)
            {
                if(data[i] == 'R')
                {
                    Console.WriteLine($"i:{i} -- {(char)data[i + 1]}, {(char)data[i + 2]}, {(char) data[i + 3]}");
                }
            }

            Console.WriteLine($"Byte array: {string.Join(" ", data)}");

            Console.WriteLine($"Length: {len}");

            return 0;
        }
    }
}