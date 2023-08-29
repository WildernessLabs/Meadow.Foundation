using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class HC2
    {
        //The baud rate is 19200, 8 bits, no parity, with one stop bit
        readonly ISerialMessagePort? serialPort;

        static readonly byte[] suffixDelimiter = { 13 }; //ASCII return
        static readonly int portSpeed = 19200;

        DateTime lastUpdate = DateTime.MinValue;

        /// <summary>
        /// Creates a new HC2 object communicating over serial
        /// </summary>
        /// <param name="device">The device connected to the sensor</param>
        /// <param name="serialPort">The serial port</param>
        public HC2(IMeadowDevice device, SerialPortName serialPort)
            : this(device.CreateSerialMessagePort(serialPort, suffixDelimiter, false, baudRate: portSpeed))
        { }

        /// <summary>
        /// Creates a new HC2 object communicating over serial
        /// </summary>
        /// <param name="serialMessagePort">The serial message port</param>
        public HC2(ISerialMessagePort serialMessagePort)
        {
            serialPort = serialMessagePort;
            serialPort.MessageReceived += SerialMessagePort_MessageReceived;

            communicationType = CommunicationType.Serial;
        }

        (Units.RelativeHumidity?, Units.Temperature?) ReadSensorSerial()
        {
            if (serialPort == null)
                return (null, null);

            if (!serialPort.IsOpen)
                serialPort.Open();

            // Send command to read
            serialPort.Write(Encoding.ASCII.GetBytes(CommandRead));

            Thread.Sleep(500);  // The sensor should respond in 500ms or less

            return Conditions;
        }

        private void SerialMessagePort_MessageReceived(object sender, SerialMessageData e)
        {
            var message = e.GetMessageString(System.Text.Encoding.ASCII);

            if (message[0] != '{')
            return;

            // split response into parsable fields
            string[] fields = message.Split(';');

            // parse the useful fields
            double.TryParse(fields[(int)ResponseFields.RHValue].Trim(), out var humidity);
            double.TryParse(fields[(int)ResponseFields.TempValue].Trim(), out var temperature);

            // Prepare Change Result
            var oldConditions = Conditions;
            var newConditions = (new Units.RelativeHumidity(humidity), new Units.Temperature(temperature, Units.Temperature.UnitType.Celsius));

            ChangeResult<(Units.RelativeHumidity? Humidity, Units.Temperature? Temperature)> changeResult = new ChangeResult<(Units.RelativeHumidity? Humidity, Units.Temperature? Temperature)>()
            {
                New = newConditions,
                Old = oldConditions,
            };

            Conditions = newConditions;

            if (UpdateInterval == null || DateTime.Now - lastUpdate >= UpdateInterval)
            {
                lastUpdate = DateTime.Now;
                RaiseEventsAndNotify(changeResult);
            }
        }
    }
}