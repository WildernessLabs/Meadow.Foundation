using System;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    
    public partial class MaxBotix
    {
        //The baud rate is 9600, 8 bits, no parity, with one stop bit
        readonly ISerialMessagePort serialMessagePort;

        static readonly byte[] suffixDelimiter = { 13 }; //ASCII return
        static readonly int portSpeed = 9600; //this is fixed for MaxBotix

        /// <summary>
        /// Creates a new MaxBotix object communicating over serial
        /// </summary>
        /// <param name="device">The device conected to the sensor</param>
        /// <param name="serialPort">The serial port</param>
        /// <param name="sensor">The distance sensor type</param>
        public MaxBotix(IMeadowDevice device, SerialPortName serialPort, SensorType sensor)
            : this(device.CreateSerialMessagePort(serialPort, suffixDelimiter, false, baudRate: portSpeed), sensor)
        { }

        /// <summary>
        /// Creates a new MaxBotix object communicating over serial
        /// </summary>
        /// <param name="serialMessage">The serial message port</param>
        /// <param name="sensor">The distance sensor type</param>
        public MaxBotix(ISerialMessagePort serialMessage, SensorType sensor)
        {
            serialMessagePort = serialMessage;
            serialMessagePort.MessageReceived += SerialMessagePort_MessageReceived;

            communication = CommunicationType.Serial;
            sensorType = sensor;
        }

        Length ReadSensorSerial()
        {   
            return Distance.Value;
        }

        private void SerialMessagePort_MessageReceived(object sender, SerialMessageData e)
        {
            //R###\n //cm
            //R####\n //mm
            //R####\n //cm
            //need to check inches
            var message = e.GetMessageString(System.Text.Encoding.ASCII);

            if (message[0] != 'R')
            { return; }

            //strip the leading R
            string cleaned = message.Substring(1);

            // get index of space
            var spaceIndex = message.FirstIndexOf(new char[] { ' ' });
            if (spaceIndex > 0)
            {
                cleaned = cleaned.Substring(0, spaceIndex);
            }

            var value = double.Parse(cleaned);

            Length.UnitType units = GetUnitsForSensor(sensorType);

            ChangeResult<Length> changeResult = new ChangeResult<Length>()
            {
                New = new Length(value, units),
                Old = Distance,
            };
           
            Distance = changeResult.New;
            RaiseEventsAndNotify(changeResult);
        }
    }
}