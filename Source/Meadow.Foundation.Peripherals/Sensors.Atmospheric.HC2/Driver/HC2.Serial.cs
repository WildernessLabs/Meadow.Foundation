using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class HC2
    {
        //The baud rate is 19200, 8 bits, no parity, with one stop bit
        readonly ISerialMessagePort? serialPort;
        static readonly int portSpeed = 19200;

        static readonly byte[] suffixDelimiter = Encoding.ASCII.GetBytes("\r"); // All response messages end with \r
        static readonly int bufferSize = 128;                                   // Typical response is 100 to 110 bytes

        /// <summary>
        /// Flag to track whether a serial command has been sent that has not yet received a response.
        /// </summary>
        internal bool RequestPending { get; set; }
        internal char DeviceID { get; set; } = ' ';
        internal int DeviceAdress { get; set; } = 99; // 99 is the broadcast address
        private const string ReadCommand = "RDD";

        DateTime lastUpdate = DateTime.MinValue;

        /// <summary>
        /// Creates a new HC2 object communicating over serial
        /// </summary>
        /// <param name="device">The device connected to the sensor</param>
        /// <param name="serialPort">The serial port</param>
        public HC2(IMeadowDevice device, SerialPortName serialPort)
            : this(device.CreateSerialMessagePort(serialPort, suffixDelimiter, false, baudRate: portSpeed, readBufferSize: bufferSize))
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

        /// <summary>
        /// Issues serial message to HC2, and tries to wait for response to arrive before returning the values.
        /// </summary>
        /// <returns></returns>
        (Units.RelativeHumidity?, Units.Temperature?) ReadSensorSerial()
        {
            if (serialPort == null)
                return (null, null);

            if (!serialPort.IsOpen)
                serialPort.Open();

            // Reqest already pending, don't queue a new one yet. 
            if (RequestPending)
                return (null, null);  // Could alro respond with current Conditions?
            // Send command to request data
            var readRequest = $"{{{DeviceID}{DeviceAdress}{ReadCommand}}}\r";
            var buffer = Encoding.ASCII.GetBytes(readRequest);
            serialPort.Write(buffer);
            RequestPending = true;

            // The sensor should respond in 500ms or less, but allow some time for the response to get handled before giving up.
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(100);  
                if (!RequestPending)
                    break;
            }

            return Conditions;
        }

        private void SerialMessagePort_MessageReceived(object sender, SerialMessageData e)
        {
            var message = e.GetMessageString(System.Text.Encoding.ASCII);

            // example response from real device
            // {F00rdd 001; 42.47;%rh;000;+; 23.31;°C;000;-;nc;---.- ;°C;000; ;001;V1.4-1;0060257484;HygroClip 2 ;000;R\r

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
            RequestPending = false;

            if (UpdateInterval == null || DateTime.Now - lastUpdate >= UpdateInterval)
            {
                lastUpdate = DateTime.Now;
                if (!IsSampling)
                    RaiseEventsAndNotify(changeResult); // Only raise events directly if perodic sampling is not enabled, as sampling will also raise the event.
            }
        }

        private char CalculateChecksum(string data)
        {
            int sum = 0;
            char[] trimChars = new char[2] { '}', '\n' };
            foreach (char c in data.TrimEnd(trimChars))
            { 
                sum += (byte)c;
            }
            return (char)(sum % 0x40 + 0x20);
        }
    }
}