using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        readonly ISerialMessagePort serialMessagePort;

        static readonly byte[] suffixDelimiter = { 13 }; //ASCII return
        static readonly int portSpeed = 9600; //this is fixed for MaxBotix

        //The baud rate is 9600, 8 bits, no parity, with one stop bit
        public MaxBotix(IMeadowDevice device, SerialPortName serialPort, SensorType sensor)
            : this(device.CreateSerialMessagePort(serialPort, suffixDelimiter, false, baudRate: portSpeed), sensor)
        {
        }

        public MaxBotix(ISerialMessagePort serialMessage, SensorType sensor)
        {
            serialMessagePort = serialMessage;
            serialMessagePort.MessageReceived += SerialMessagePort_MessageReceived;

            communication = CommunicationType.Serial;
            sensorType = sensor;
        }

        Length ReadSensorSerial()
        {   //I think we'll just cache it for serial
            return Length.Value;
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

            //it'll throw an exception if it's wrong
            //strip the leading R
            var value = double.Parse(message.Substring(1));

            //need to get this per sensor
            Length.UnitType units = GetUnitsForSensor(sensorType);

            // create a new change result from the new value
            ChangeResult<Length> changeResult = new ChangeResult<Length>()
            {
                New = new Length(value, units),
                Old = Length,
            };
            // save state
            Length = changeResult.New;
            // notify
            RaiseEventsAndNotify(changeResult);
        }
    }
}