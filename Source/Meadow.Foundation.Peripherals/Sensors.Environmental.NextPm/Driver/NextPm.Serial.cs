using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents a TERA Sensor NextPM particulate matter sensor
    /// </summary>
    public partial class NextPm : SamplingSensorBase<int>
    {
        private ISerialPort _port;
        // dev note: these common buffers are used to minimize heap allocations during serial communications with the sensor
        private byte[] _readBuffer = new byte[16];
        private byte[] _writeBuffer = new byte[16];

        private ISerialPort InitializePort(ISerialPort port)
        {
            if (port.IsOpen)
            {
                port.Close();
            }

            port.BaudRate = 115200;
            port.DataBits = 8;
            port.Parity = Parity.Even;
            port.StopBits = StopBits.One;

            return port;
        }

        private async Task SendCommand(CommandByte command, params byte[] payload)
        {
            var parameters = CommandManager.GenerateCommand(command, _writeBuffer, payload);

            if (!_port.IsOpen)
            {
                _port.Open();
            }

            _port.Write(_writeBuffer, 0, parameters.commandLength);

            var read = 0;
            var expected = parameters.expectedResponseLength;
            var timeoutMs = 3000;
            var readDelay = 500;

            await Task.Delay(100); // initial device response delay

            Array.Clear(_readBuffer, 0, _readBuffer.Length);

            do
            {
                read += _port.Read(_readBuffer, read, _readBuffer.Length - read);
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