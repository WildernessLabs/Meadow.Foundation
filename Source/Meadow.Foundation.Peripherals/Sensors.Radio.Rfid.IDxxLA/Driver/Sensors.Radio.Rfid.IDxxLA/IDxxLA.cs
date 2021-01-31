using System;
using System.Collections.Generic;
using System.Diagnostics;
using Meadow.Foundation.Sensors.Radio.Rfid.Serial.Helpers;
using Meadow.Foundation.Helpers;
using Meadow.Hardware;
using Meadow.Utilities;

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
        public const int BaudRate = 9600;
        public const int DataBits = 7;

        public ISerialMessagePort SerialPort { get; }

        private const byte StartToken = 2;
        private const byte EndToken = 3;

        private readonly IList<IObserver<byte[]>> _observers = new List<IObserver<byte[]>>();

        /// <inheritdoc />
        public event RfidReadEventHandler RfidRead = delegate { };

        /// <summary>
        /// Create an IDxxLA RFID reader
        /// </summary>
        /// <param name="device">Device to use</param>
        /// <param name="serialPortName">Port name to use</param>
        public IDxxLA(IIODevice device, SerialPortName serialPortName) :
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
            foreach (var observer in _observers) {
                observer?.OnCompleted();
            }
            _observers.Clear();

            if (SerialPort.IsOpen) {
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
            lock (_observers) {
                if (!_observers.Contains(observer)) {
                    _observers.Add(observer);
                }
            }

            return new Unsubscriber(_observers, observer);
        }

        private static byte[] AsciiHexByteArrayToBytes(Span<byte> hexBytes)
        {
            if (hexBytes.Length % 2 != 0) {
                throw new ArgumentException(
                    "Byte array must contain an event number of bytes to be parsed",
                    nameof(hexBytes));
            }

            var result = new byte[hexBytes.Length / 2];
            for (var i = 0; i < hexBytes.Length; i += 2) {
                result[i / 2] = (byte)((AsciiHexByteToByte(hexBytes[i]) << 4) | AsciiHexByteToByte(hexBytes[i + 1]));
            }

            return result;
        }

        private static byte AsciiHexByteToByte(byte hexByte)
        {
            switch (hexByte) {
                case 48:
                    return 0;
                case 49:
                    return 1;
                case 50:
                    return 2;
                case 51:
                    return 3;
                case 52:
                    return 4;
                case 53:
                    return 5;
                case 54:
                    return 6;
                case 55:
                    return 7;
                case 56:
                    return 8;
                case 57:
                    return 9;
                case 65:
                    return 10;
                case 66:
                    return 11;
                case 67:
                    return 12;
                case 68:
                    return 13;
                case 69:
                    return 14;
                case 70:
                    return 15;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(hexByte),
                        hexByte,
                        "Value must be a valid ASCII representation of a hex character (0-F)");
            }
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

            if (data.Length != validLength) {
                Debug.WriteLine(
                    $"Serial data is not of expected length for RFID tag format. Expected {validLength}, actual {data.Length}");
                return (tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            if (data[startByte] != StartToken) {
                Debug.WriteLine(
                    $"Invalid start byte in serial data for RFID tag format. Expected '{StartToken}', actual '{data[startByte]}'");
                return (tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            if (data[endByte] != EndToken) {
                Debug.WriteLine(
                    $"Invalid end byte in serial data for RFID tag format. Expected '{EndToken}', actual '{data[endByte]}'");
                return (tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            // verify tag is hexadecimal
            var tagSlice = data.Slice(tagStartByte, tagLength);
            if (!IsHexChars(tagSlice)) {
                Debug.WriteLine(
                    "Invalid end byte in serial data for RFID tag format. Expected hex ASCII character (48-57, 65-70)");
                return (tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            // verify checksum is hexadecimal
            var checksumSlice = data.Slice(checksumStartByte, checksumLength);
            if (!IsHexChars(checksumSlice)) {
                Debug.WriteLine(
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
            foreach (var asciiByte in asciiBytes) {
                if (!(asciiByte >= 48 && asciiByte <= 57) && !(asciiByte >= 65 && asciiByte <= 70)) {
                    return false;
                }
            }

            return true;
        }

        private void OnTagReadEvent(RfidValidationStatus status, byte[] tag)
        {
            if (status == RfidValidationStatus.Ok) {
                LastRead = tag;
            }

            RfidRead(this, new RfidReadResult { Status = status, RfidTag = tag });
            foreach (var observer in _observers) {
                if (status == RfidValidationStatus.Ok) {
                    observer?.OnNext(tag);
                } else {
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
                lock (_observers) {
                    if (_observer != null) {
                        _observers?.Remove(_observer);
                    }
                }
            }
        }
    }
}
