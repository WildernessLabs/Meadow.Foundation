﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Max7219 LED matrix driver
    /// </summary>
    public class Max7219
    {
        #region Properties

        /// <summary>
        /// MAX7219 Spi Clock Frequency
        /// </summary>
        public int SpiClockFrequency => 10000000;

        /// <summary>
        /// Number of digits Register per Module
        /// </summary>
        public const int NumDigits = 8;

        /// <summary>
        /// Number of cascaded devices
        /// </summary>
        /// <value></value>
        public int DeviceCount { get; private set; }

        #endregion Properties

        #region Member variables / fields

        private ISpiPeripheral max7219;

        /// <summary>
        /// internal buffer used to write to registers for all devices.
        /// </summary>
        private readonly byte[] _writeBuffer;

        /// <summary>
        /// A Buffer that contains the values of the digits registers per device
        /// </summary>
        private readonly byte[,] _buffer;

        #endregion Member variables / fields

        #region Enums

        internal enum Register : byte
        {
            NOOP        = 0x00,
            DIGIT0      = 0x01,
            DIGIT1      = 0x02,
            DIGIT2      = 0x03,
            DIGIT3      = 0x04,
            DIGIT4      = 0x05,
            DIGIT5      = 0x06,
            DIGIT6      = 0x07,
            DIGIT7      = 0x08,
            DECODEMODE  = 0x09,
            INTENSITY   = 0x0A,
            SCANLIMIT   = 0x0B,
            SHUTDOWN    = 0x0C,
            DISPLAYTEST = 0x0F
        }

        #endregion Enums

        #region Constructors

        private Max7219() { }

        /// <summary>
        /// Creates a Max7219 Device given a <see paramref="spiDevice" /> to communicate over and the
        /// number of devices that are cascaded.
        /// </summary>
        public Max7219(IIODevice device, ISpiBus spiBus, IPin csPin, int deviceCount = 1)
        {
            var csPort = device.CreateDigitalOutputPort(csPin);
            max7219 = new SpiPeripheral(spiBus, csPort);
          
            DeviceCount = deviceCount;

            _buffer = new byte[DeviceCount, NumDigits];
            _writeBuffer = new byte[2 * DeviceCount];

            Initialize();
        }

        #endregion Constructors


        /// <summary>
        /// Standard initialization routine.
        /// </summary>
        void Initialize()
        {
            SetRegister(Register.SCANLIMIT, 7); //show all 8 digits
            SetRegister(Register.DECODEMODE, 0); // use matrix (not digits)
            SetRegister(Register.DISPLAYTEST, 0); // no display test
            SetRegister(Register.SHUTDOWN, 1); // not shutdown mode
            Brightness(4); //intensity, range: 0..15
            ClearAll();
        }

        public void TestDisplay(int timeInMs = 1000)
        {
            SetRegister(Register.DISPLAYTEST, 0xFF);

            Thread.Sleep(timeInMs);

            SetRegister(Register.DISPLAYTEST, 0); 
        }


        /// <summary>
        /// Sends data to a specific register replicated for all cascaded devices
        /// </summary>
        internal void SetRegister(Register register, byte data)
        {
            var i = 0;
            for (byte deviceId = 0; deviceId < DeviceCount; deviceId++)
            {
                _writeBuffer[i++] = (byte)register;
                _writeBuffer[i++] = data;
            }
            max7219.WriteBytes(_writeBuffer);
        }



        /// <summary>
        /// Sets the brightness of all cascaded devices to the same intensity level.
        /// </summary>
        /// <param name="intensity">intensity level ranging from 0..15. </param>
        public void Brightness(int intensity)
        {
            if (intensity < 0 || intensity > 15)
                throw new ArgumentOutOfRangeException(nameof(intensity), $"Invalid intensity for Brightness {intensity}");
            SetRegister(Register.INTENSITY, (byte)intensity);
        }

        /// <summary>
        /// Gets or Sets the value to the digit value for a given device
        /// and digit position
        /// </summary>
        public byte this[int deviceId, int digit]
        {
            get
            {
                ValidatePosition(deviceId, digit);
                return _buffer[deviceId, digit];
            }
            set
            {
                ValidatePosition(deviceId, digit);
                _buffer[deviceId, digit] = value;
            }
        }

        /// <summary>
        /// Gets or Sets the value to the digit value for a given absolute index
        /// </summary>
        public byte this[int index]
        {
            get
            {
                ValidateIndex(index, out var deviceId, out var digit);
                return _buffer[deviceId, digit];
            }
            set
            {
                ValidateIndex(index, out var deviceId, out var digit);
                _buffer[deviceId, digit] = value;
            }
        }

        /// <summary>
        /// Gets the total number of digits (cascaded devices * num digits)
        /// </summary>
        public int Length => DeviceCount * NumDigits;

        private void ValidatePosition(int deviceId, int digit)
        {
            if (deviceId < 0 || deviceId >= DeviceCount)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId), $"Invalid device Id: {deviceId}");
            }
            if (digit < 0 || digit >= NumDigits)
            {
                throw new ArgumentOutOfRangeException(nameof(digit), $"Invalid digit: {digit}");
            }
        }

        private void ValidateIndex(int index, out int deviceId, out int digit)
        {
            if (index < 0 || index > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Invalid index {index}");
            }
            deviceId = Math.DivRem(index, NumDigits, out digit);
        }

        /// <summary>
        /// Writes all the Values to the devices.
        /// </summary>
        public void Show()
        {
            WriteBuffer(_buffer);
        }

        /// <summary>
        /// Writes a two dimensional buffer containing all the values to the devices.
        /// </summary>
        public void WriteBuffer(byte[,] buffer)
        {
            ValidateBuffer(buffer);

            for (int digit = 0; digit < NumDigits; digit++)
            {
                var i = 0;

                for (var deviceId = DeviceCount - 1; deviceId >= 0; deviceId--)
                {
                    _writeBuffer[i++] = (byte)((int)Register.DIGIT0 + digit);
                    _writeBuffer[i++] = buffer[deviceId, digit];
                }
                max7219.WriteBytes(_writeBuffer);
            }
        }

        /// <summary>
        /// Validates the buffer dimensions.
        /// </summary>
        /// <param name="buffer"></param>
        private void ValidateBuffer(byte[,] buffer)
        {
            if (buffer.Rank != 2)
            {
                throw new ArgumentException(nameof(buffer), $"buffer must be two dimensional.");
            }
            if (buffer.GetUpperBound(0) != DeviceCount - 1)
            {
                throw new ArgumentException(nameof(buffer), $"buffer upper bound ({buffer.GetUpperBound(0)}) for dimension 0 must be {DeviceCount - 1}.");
            }
            if (buffer.GetUpperBound(1) != NumDigits - 1)
            {
                throw new ArgumentException(nameof(buffer), $"buffer upper bound ({buffer.GetUpperBound(1)}) for dimension 1 must be {NumDigits - 1}.");
            }
        }

        /// <summary>
        /// Clears the buffer from the given start to end (exclusive) and flushes
        /// </summary>
        public void Clear(int start, int end, bool update = true)
        {
            if (end < 0 || end > DeviceCount)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }

            if (start < 0 || start >= end)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }

            for (int deviceId = start; deviceId < end; deviceId++)
            {
                for (int digit = 0; digit < NumDigits; digit++)
                {
                    this[deviceId, digit] = 0;
                }
            }

            if (update)
            {
                Show();
            }
        }

        /// <summary>
        /// Clears the buffer from the given start to end and flushes
        /// </summary>
        public void ClearAll(bool flush = true)
        {
            Clear(0, DeviceCount, flush);
        }
    }
}