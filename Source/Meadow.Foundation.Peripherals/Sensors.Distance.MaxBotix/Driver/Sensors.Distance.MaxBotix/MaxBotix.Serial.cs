using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        ISerialMessagePort serialMessagePort;

        //The baud rate is 9600, 8 bits, no parity, with one stop bit
        public MaxBotix(IMeadowDevice device, SerialPortName serialPort, SensorModel sensor,
            int serialPortSpeed = 9600)
            : this(device.CreateSerialMessagePort(serialPort, new byte[] { 13 }, false), sensor)
        {
        }

        public MaxBotix(ISerialMessagePort serialMessage, SensorModel sensor)
        {
            serialMessagePort = serialMessage;
            serialMessagePort.MessageReceived += SerialMessagePort_MessageReceived;

            communication = CommunicationType.Serial;
            sensorModel = sensor;
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
            Length.UnitType units = Meadow.Units.Length.UnitType.Millimeters;

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