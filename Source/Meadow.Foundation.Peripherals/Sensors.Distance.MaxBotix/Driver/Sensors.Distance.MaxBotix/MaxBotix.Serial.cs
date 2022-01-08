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
            Length.UnitType units = GetUnitsForDevice(sensorModel);

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

        Length.UnitType GetUnitsForDevice(SensorModel sensor)
        {
            switch(sensor)
            {
                case SensorModel.MB1000:
                case SensorModel.MB1010:
                case SensorModel.MB1020:
                case SensorModel.MB1030:
                case SensorModel.MB1040:
                    return Units.Length.UnitType.Inches;

                case SensorModel.MB1200:
                case SensorModel.MB1210:
                case SensorModel.MB1220:
                case SensorModel.MB1230:
                case SensorModel.MB1240:
                case SensorModel.MB1260:
                case SensorModel.MB1261:
                case SensorModel.MB1300:
                case SensorModel.MB1310:
                case SensorModel.MB1320:
                case SensorModel.MB1330:
                case SensorModel.MB1340:
                case SensorModel.MB1360:
                case SensorModel.MB1361:

                case SensorModel.MB2530:
                case SensorModel.MB2532:
                    return Units.Length.UnitType.Centimeters;

                default:
                    //most are mm so we'll use it as the default
                    return Units.Length.UnitType.Millimeters;
            }
        }
    }
}