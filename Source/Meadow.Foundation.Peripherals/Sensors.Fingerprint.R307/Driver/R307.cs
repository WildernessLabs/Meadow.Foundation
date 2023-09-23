using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Fingerprint
{
    /// <summary>
    /// Represents a Rajguru R307 serial fingerprint sensor
    /// </summary>
    public partial class R307 : IDisposable
    {
        public const int DefaultBaudRate = 57600;

        private readonly ISerialPort _serialPort;
        private IDigitalInterruptPort? _detectPort;

        private uint _password;

        public bool IsDisposed { get; private set; }

        public R307(ISerialPort serialPort, IPin? detectPin, uint password = 0)
        {
            _serialPort = serialPort;
            _password = password;

            //            _serialPort.DataReceived += OnSerialDataReceived;

            if (detectPin != null)
            {
                _detectPort = detectPin.CreateDigitalInterruptPort(InterruptMode.EdgeFalling, ResistorMode.ExternalPullUp);
                _detectPort.Changed += OnFingerDetected;
            }

            Initialize();
        }

        public void Enable()
        {
            Resolver.Log.Debug("Enabling R307");

            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
            if (!CheckPassword(_password))
            {
                throw new Exception("Failed");
            }

            Resolver.Log.Debug("Enabled R307");
        }

        public void ReadFingerprint()
        {
        }

        private bool CheckPassword(uint password)
        {
            Span<byte> payload = stackalloc byte[4];
            payload[0] = (byte)((password >> 24) & 0xff);
            payload[1] = (byte)((password >> 16) & 0xff);
            payload[2] = (byte)((password >> 8) & 0xff);
            payload[3] = (byte)((password >> 0) & 0xff);

            var packet = new CommandPacket(CommandCode.VerifyPassword, payload);

            var response = SendCommand(packet);

            switch (response)
            {
                case ConfirmationCode.ExecutionComplete:
                    return true;
                default:
                    return false;
            }
        }

        private ConfirmationCode SendCommand(Packet packet)
        {
            Resolver.Log.Debug("Sending command...");

            _serialPort.Write(packet.Serialize());

            // response is always a 9-byte header + payload + CRC.  Max payload is 256
            var buffer = new byte[267];
            var read = 0;

            Resolver.Log.Debug($"reading");

            var tryCount = 0;

            while (read < 9)
            {
                var r = _serialPort.Read(buffer, read, 9 - read);
                if (r == 0)
                {
                    System.Threading.Thread.Sleep(100);
                    if (tryCount++ > 50)
                    {
                        throw new TimeoutException();
                    }
                }
                Resolver.Log.Debug($"reading {read}");

                read += _serialPort.Read(buffer, read, 9 - read);
            }

            var packetType = (PacketIdentifier)buffer[6];
            var length = (buffer[7] << 8) | buffer[8];

            while (read < length)
            {
                read += _serialPort.Read(buffer, read, length - read);
            }

            Resolver.Log.Debug($"received {length} bytes");

            // TODO: check the CRC

            return ConfirmationCode.ExecutionComplete;
        }

        private void ReadParameters()
        {
        }

        private void OnFingerDetected(object sender, DigitalPortResult e)
        {
            Resolver.Log.Debug("Finger detected");

            ReadFingerprint();
        }

        private void Initialize()
        {
        }

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_detectPort != null)
                    {
                        _detectPort.Dispose();
                        _detectPort = null;
                    }
                }

                IsDisposed = true;
            }
        }


        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}