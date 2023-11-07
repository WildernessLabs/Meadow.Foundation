using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents a TERA Sensor NextPM particulate matter sensor
    /// </summary>
    public partial class NextPm
    {
        private readonly ISerialPort? serialPort;

        /// <summary>
        /// Creates a NextPm instance
        /// </summary>
        /// <param name="portName">The serial serialPort name</param>
        public NextPm(SerialPortName portName)
            : this(portName.CreateSerialPort())
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a NextPm instance
        /// </summary>
        /// <param name="serialPort">The serial serialPort</param>
        public NextPm(ISerialPort serialPort)
        {
            InitializeSerialPort();

            if (serialPort != null)
            {
                this.serialPort = serialPort;
                serialPort.Open();
            }
        }

        // dev note: these common buffers are used to minimize heap allocations during serial communications with the sensor
        private readonly byte[] _readBuffer = new byte[16];
        private readonly byte[] _writeBuffer = new byte[16];

        private ISerialPort InitializeSerialPort()
        {
            if (serialPort!.IsOpen)
            {
                serialPort.Close();
            }

            serialPort.BaudRate = 115200;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.Even;
            serialPort.StopBits = StopBits.One;

            return serialPort;
        }

        private async Task SendCommand(CommandByte command, params byte[] payload)
        {
            var parameters = CommandManager.GenerateCommand(command, _writeBuffer, payload);

            if (serialPort!.IsOpen)
            {
                serialPort.Open();
            }

            serialPort.Write(_writeBuffer, 0, parameters.commandLength);

            var read = 0;
            var expected = parameters.expectedResponseLength;
            var timeoutMs = 3000;
            var readDelay = 500;

            await Task.Delay(100); // initial device response delay

            Array.Clear(_readBuffer, 0, _readBuffer.Length);

            do
            {
                read += serialPort.Read(_readBuffer, read, _readBuffer.Length - read);
                if (read >= expected) break;

                if (read > 2 && _readBuffer[1] != (byte)command && _readBuffer[1] == 0x16)
                {
                    var status = (SensorStatus)_readBuffer[2];
                    // typically a "fail" due to the device being in sleep state
                    throw new TeraException($"Device status: {status}");
                }

                await Task.Delay(readDelay);
                timeoutMs -= readDelay;
            } while (timeoutMs > 0);

            if (read < expected)
            {
                throw new TeraException("Serial timeout");
            }
            else
            {
                var checksumIsValid = CommandManager.ValidateChecksum(_readBuffer, 0, parameters.expectedResponseLength);

                if (!checksumIsValid)
                {
                    throw new TeraException("Invalid response checksum");
                }
            }
        }
    }
}