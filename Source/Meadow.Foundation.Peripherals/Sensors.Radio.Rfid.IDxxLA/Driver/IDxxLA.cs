using Meadow.Hardware;
using Meadow.Utilities;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Radio.Rfid
{
    /// <summary>
    /// RFID reader for ID-2LA, ID-12LA and ID-20LA serial readers.
    /// </summary>
    /// <remarks>
    /// Only supports reading ASCII output formats. Magnet emulation and Wiegand26 are not supported.
    /// Based on the datasheet, this code should also work for the non-LA variants of the RFID readers. The only significant
    /// change is the different voltage that the device will need to be supplied with.
    /// </remarks>
    public class IDxxLA : IRfidReader
    {
        /// <summary>
        /// The baud rate (9600)
        /// </summary>
        public const int BaudRate = 9600;

        /// <summary>
        /// Data bits (7)
        /// </summary>
        public const int DataBits = 7;

        ISerialMessagePort SerialPort { get; }

        const byte StartToken = 2;
        const byte EndToken = 3;

        readonly IList<IObserver<byte[]>> _observers = new List<IObserver<byte[]>>();

        /// <inheritdoc />
        public event RfidReadEventHandler RfidRead = delegate { };

        /// <summary>
        /// Create an IDxxLA RFID reader
        /// </summary>
        /// <param name="device">Device to use</param>
        /// <param name="serialPortName">Port name to use</param>
        public IDxxLA(ISerialMessageController device, SerialPortName serialPortName) :
            this(device.CreateSerialMessagePort(
                    serialPortName,
                    suffixDelimiter: new byte[] { EndToken },
                    preserveDelimiter: true,
                    baudRate: BaudRate,
                    dataBits: DataBits
                    )
                )
        {
        }

        /// <summary>
        /// Create an IDxxLA RFID reader using an existing port.
        /// </summary>
        /// <param name="serialPort"></param>
        /// <remarks>
        /// Be sure to use suitable settings when creating the serial port.
        /// Default <see cref="BaudRate" /> and <see cref="DataBits" /> are exposed as constants.
        /// </remarks>
        public IDxxLA(ISerialMessagePort serialPort)
        {
            SerialPort = serialPort;

            SerialPort.MessageReceived += SerialPort_MessageReceived;
        }

        private void SerialPort_MessageReceived(object sender, SerialMessageData e)
        {
            var (tag, status) = GetValidatedRfidTag(e.Message);
            OnTagReadEvent(status, tag);
        }

        /// <inheritdoc />
        public byte[] LastRead { get; private set; }

        /// <summary>
        /// Dispose of this instance.
        /// </summary>
        public void Dispose()
        {
            foreach (var observer in _observers)
            {
                observer?.OnCompleted();
            }
            _observers.Clear();

            if (SerialPort.IsOpen)
            {
                SerialPort.Close();
            }
        }

        /// <inheritdoc />
        public void StartReading()
        {
            SerialPort.Open();
        }

        /// <inheritdoc />
        public void StopReading()
        {
            SerialPort.Close();
        }

        /// <summary>
        /// Subscribe to RFID tag reads.
        /// Observer will only receive valid reads, with invalid reads triggering an OnError call.
        /// OnComplete will be called if this instance is disposed.
        /// This call is thread-safe.
        /// </summary>
        /// <param name="observer">The observer to subscribe</param>
        /// <returns>Disposable unsubscriber</returns>
        public IDisposable Subscribe(IObserver<byte[]> observer)
        {
            // Ensure thread safety
            // See https://docs.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern-best-practices
            lock (_observers)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
            }

            return new Unsubscriber(_observers, observer);
        }

        private static byte[] AsciiHexByteArrayToBytes(Span<byte> hexBytes)
        {
            if (hexBytes.Length % 2 != 0)
            {
                throw new ArgumentException(
                    "Byte array must contain an event number of bytes to be parsed",
                    nameof(hexBytes));
            }

            var result = new byte[hexBytes.Length / 2];
            for (var i = 0; i < hexBytes.Length; i += 2)
            {
                result[i / 2] = (byte)((AsciiHexByteToByte(hexBytes[i]) << 4) | AsciiHexByteToByte(hexBytes[i + 1]));
            }

            return result;
        }

        private static byte AsciiHexByteToByte(byte hexByte)
        {
            return hexByte switch
            {
                48 => 0,
                49 => 1,
                50 => 2,
                51 => 3,
                52 => 4,
                53 => 5,
                54 => 6,
                55 => 7,
                56 => 8,
                57 => 9,
                65 => 10,
                66 => 11,
                67 => 12,
                68 => 13,
                69 => 14,
                70 => 15,
                _ => throw new ArgumentOutOfRangeException(
                                        nameof(hexByte),
                                        hexByte,
                                        "Value must be a valid ASCII representation of a hex character (0-F)"),
            };
        }

        private static (byte[] tag, RfidValidationStatus status) GetValidatedRfidTag(Span<byte> data)
        {
            // Valid format is as follows:
            // STX, 0-F x10 tag, 0-F x2 checksum, CR, LF, ETX
            // example:
            // STX 7 C 0 0 5 5 F 8 C 4 1 5 CR LF ETX
            const int validLength = 16;
            const int startByte = 0;
            const int endByte = 15;
            const int tagStartByte = 1;
            const int tagLength = 10;
            const int checksumStartByte = 11;
            const int checksumLength = 2;

            if (data.Length != validLength)
            {
                Console.WriteLine(
                    $"Serial data is not of expected length for RFID tag format. Expected {validLength}, actual {data.Length}");
                return (tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            if (data[startByte] != StartToken)
            {
                Console.WriteLine(
                    $"Invalid start byte in serial data for RFID tag format. Expected '{StartToken}', actual '{data[startByte]}'");
                return (tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            if (data[endByte] != EndToken)
            {
                Console.WriteLine(
                    $"Invalid end byte in serial data for RFID tag format. Expected '{EndToken}', actual '{data[endByte]}'");
                return (tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            // verify tag is hexadecimal
            var tagSlice = data.Slice(tagStartByte, tagLength);
            if (!IsHexChars(tagSlice))
            {
                Console.WriteLine(
                    "Invalid end byte in serial data for RFID tag format. Expected hex ASCII character (48-57, 65-70)");
                return (tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            // verify checksum is hexadecimal
            var checksumSlice = data.Slice(checksumStartByte, checksumLength);
            if (!IsHexChars(checksumSlice))
            {
                Console.WriteLine(
                    "Invalid end byte in serial data for RFID tag format. Expected hex ASCII character (48-57, 65-70)");
                return (tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            var tag = AsciiHexByteArrayToBytes(tagSlice);
            var checkSum = AsciiHexByteArrayToBytes(checksumSlice)[0];

            return (ChecksumCalculator.XOR(tag) == checkSum)
                ? (tag, status: RfidValidationStatus.Ok)
                : (tag, status: RfidValidationStatus.ChecksumFailed);
        }

        private static bool IsHexChars(Span<byte> asciiBytes)
        {
            foreach (var asciiByte in asciiBytes)
            {
                if (!(asciiByte >= 48 && asciiByte <= 57) && !(asciiByte >= 65 && asciiByte <= 70))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnTagReadEvent(RfidValidationStatus status, byte[] tag)
        {
            if (status == RfidValidationStatus.Ok)
            {
                LastRead = tag;
            }

            RfidRead(this, new RfidReadResult { Status = status, RfidTag = tag });
            foreach (var observer in _observers)
            {
                if (status == RfidValidationStatus.Ok)
                {
                    observer?.OnNext(tag);
                }
                else
                {
                    observer?.OnError(new RfidValidationException(status));
                }
            }
        }

        private class Unsubscriber : IDisposable
        {
            private readonly IObserver<byte[]> _observer;
            private readonly IList<IObserver<byte[]>> _observers;

            public Unsubscriber(IList<IObserver<byte[]>> observers, IObserver<byte[]> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                // Ensure thread safety
                // See https://docs.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern-best-practices
                lock (_observers)
                {
                    if (_observer != null)
                    {
                        _observers?.Remove(_observer);
                    }
                }
            }
        }
    }
}