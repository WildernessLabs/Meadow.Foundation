using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using LU = Meadow.Units.Length.UnitType;

namespace Meadow.Foundation.Sensors.Distance
{
    // TODO: why is `DistanceUpdated` never invoked? is this sensor done?
    public class Mb10x0 : SensorBase<Length>, IRangeFinder
    {
        //==== events
        public event EventHandler<IChangeResult<Length>> DistanceUpdated = delegate { };

        //==== internals
        ISerialPort serialPort;

        //==== public properties
        public int Baud => 9600;

        /// <summary>
        /// The distance to the measured object.
        /// </summary>
        public Length? Distance { get; protected set; } = new Length(0);

        /// <summary>
        /// Creates a new `Mb10x0` device on the specified serial port.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="portName"></param>
        public Mb10x0(ISerialController device, SerialPortName portName)
        {
            serialPort = device.CreateSerialPort(portName, Baud);
            serialPort.Open();
        }

        /// <summary>
        /// Creates a new `Mb10x0` device on the specified serial port.
        /// </summary>
        /// <param name="serialPort"></param>
        public Mb10x0(ISerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        public Length ReadSerial()
        {
            var len = serialPort.BytesToRead;

            if(len == 0) 
            {
                Console.WriteLine("No data");
                return new Length(0, LU.Millimeters); 
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

            return new Length(0, LU.Millimeters);
        }

        protected override Task<Length> ReadSensor()
        {
            throw new NotImplementedException();
        }
    }
}