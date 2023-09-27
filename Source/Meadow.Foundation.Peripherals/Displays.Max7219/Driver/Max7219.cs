using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Max7219 LED matrix driver
    /// </summary>
    public partial class Max7219 : ISpiPeripheral
    {
        /// <summary>
        /// Number of digits per Module
        /// </summary>
        public const int DigitsPerDevice = 8;

        /// <summary>
        /// Number of cascaded devices
        /// </summary>
        public int DeviceCount => DigitRows * DigitColumns / DigitsPerDevice;

        /// <summary>
        /// Number of rows when representing digits
        /// </summary>
        public int DigitRows { get; private set; }

        /// <summary>
        /// Number of columns when representing digits 
        /// </summary>
        public int DigitColumns { get; private set; }

        /// <summary>
        /// Gets the total number of digits (cascaded devices * num digits)
        /// </summary>
        public int Length => DigitRows * DigitColumns;

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(10000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => spiComms.BusSpeed;
            set => spiComms.BusSpeed = value;
        }

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => spiComms.BusMode;
            set => spiComms.BusMode = value;
        }

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected ISpiCommunications spiComms;

        /// <summary>
        /// internal buffer used to write to registers for all devices.
        /// </summary>
        private readonly byte[] writeBuffer;

        /// <summary>
        /// A Buffer that contains the values of the digits registers per device
        /// </summary>
        private readonly byte[,] buffer;

        private readonly byte DECIMAL = 0b10000000;

        /// <summary>
        /// Create a new Max7219 object
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="chipselectPort">Chip select port</param>
        /// <param name="deviceCount">Number of cascaded devices</param>
        /// <param name="maxMode">Display mode</param>
        public Max7219(ISpiBus spiBus, IDigitalOutputPort chipselectPort, int deviceCount = 1, Max7219Mode maxMode = Max7219Mode.Display)
            : this(spiBus, chipselectPort, deviceCount, 1, maxMode)
        {
        }

        /// <summary>
        /// Create a new Max7219 object
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="chipselectPort">Chip select port</param>
        /// <param name="deviceRows">Number of devices cascaded vertically</param>
        /// <param name="deviceColumns">Number of devices cascaded horizontally</param>
        /// <param name="maxMode">Display mode</param>
        public Max7219(ISpiBus spiBus, IDigitalOutputPort chipselectPort, int deviceRows, int deviceColumns, Max7219Mode maxMode = Max7219Mode.Display)
        {
            spiComms = new SpiCommunications(spiBus, chipselectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

            DigitRows = deviceRows;
            DigitColumns = deviceColumns * DigitsPerDevice;

            buffer = new byte[DeviceCount, DigitsPerDevice];

            writeBuffer = new byte[2 * DeviceCount];

            Initialize(maxMode);
        }

        /// <summary>
        /// Create a new Max7219 object
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="deviceRows">Number of devices cascaded vertically</param>
        /// <param name="deviceColumns">Number of devices cascaded horizontally</param>
        /// <param name="maxMode">Display mode</param>
        public Max7219(ISpiBus spiBus, IPin chipSelectPin, int deviceRows = 1, int deviceColumns = 1, Max7219Mode maxMode = Max7219Mode.Display)
            : this(spiBus, chipSelectPin.CreateDigitalOutputPort(), deviceRows, deviceColumns, maxMode)
        { }

        /// <summary>
        /// Create a new Max7219 object
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="deviceCount">Number of cascaded devices</param>
        /// <param name="maxMode">Display mode</param>
        public Max7219(ISpiBus spiBus, IPin chipSelectPin, int deviceCount = 1, Max7219Mode maxMode = Max7219Mode.Display)
            : this(spiBus, chipSelectPin.CreateDigitalOutputPort(), deviceCount, 1, maxMode)
        { }

        /// <summary>
        /// Standard initialization routine.
        /// </summary>
        void Initialize(Max7219Mode maxMode)
        {
            SetRegister(Register.DecodeMode, (byte)((maxMode == Max7219Mode.Character) ? 0xFF : 0)); // use matrix(0) or digits
            SetRegister(Register.ScanLimit, 7); //show all 8 digits
            SetRegister(Register.DisplayTest, 0); // no display test
            SetRegister(Register.ShutDown, 1); // not shutdown mode
            SetBrightness(4); //intensity, range: 0..15
            Clear();
        }

        /// <summary>
        /// Set number to display (left aligned)
        /// </summary>
        /// <param name="value">the number to display</param>
        /// <param name="deviceId">the cascaded device id</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetNumber(int value, int deviceId = 0)
        {
            //12345678
            if (value > 99999999)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < 8; i++)
            {
                SetCharacter((CharacterType)(value % 10), i, false, deviceId);
                value /= 10;

                if (value == 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Set a single character
        /// </summary>
        /// <param name="character">the character to display</param>
        /// <param name="digit">the digit index starting from the left</param>
        /// <param name="showDecimal">show the decimal with the character</param>
        /// <param name="deviceId">the cascaded device id</param>
        public void SetCharacter(CharacterType character, int digit, bool showDecimal = false, int deviceId = 0)
        {
            ValidatePosition(deviceId, digit);
            var data = (byte)((byte)character + (showDecimal ? DECIMAL : 0));

            buffer[deviceId, digit] = data;
        }

        /// <summary>
        /// Get the stored character 
        /// </summary>
        /// <param name="digit">the digit index of the character</param>
        /// <param name="deviceId">the cascaded device id</param>
        /// <returns></returns>
        public CharacterType GetCharacter(int digit, int deviceId = 0)
        {
            ValidatePosition(deviceId, digit);
            return (CharacterType)buffer[deviceId, digit];
        }

        /// <summary>
        /// Helper test method - will turn all leds on then off
        /// </summary>
        /// <param name="time">time to show leds</param>
        public void TestDisplay(TimeSpan time)
        {
            SetRegister(Register.DisplayTest, 0xFF);

            Thread.Sleep((int)time.TotalMilliseconds);

            SetRegister(Register.DisplayTest, 0);
        }

        /// <summary>
        /// Set the display mode of the Max7219
        /// </summary>
        /// <param name="maxMode">the mode</param>
        public void SetMode(Max7219Mode maxMode)
        {
            SetRegister(Register.DecodeMode, (byte)((maxMode == Max7219Mode.Character) ? 0xFF : 0)); // use matrix(0) or digits
        }

        /// <summary>
        /// Sends data to a specific register replicated for all cascaded devices
        /// </summary>
        internal void SetRegister(Register register, byte data)
        {
            var i = 0;

            for (byte deviceId = 0; deviceId < DeviceCount; deviceId++)
            {
                writeBuffer[i++] = (byte)register;
                writeBuffer[i++] = data;
            }
            spiComms.Write(writeBuffer);
        }

        /// <summary>
        /// Sends data to a specific register for a specific device
        /// </summary>
        internal void SetRegister(int deviceId, Register register, byte data)
        {
            Array.Clear(writeBuffer, 0, writeBuffer.Length);

            writeBuffer[deviceId * 2] = (byte)register;
            writeBuffer[deviceId * 2 + 1] = data;

            spiComms.Write(writeBuffer);
        }

        /// <summary>
        /// Sets the brightness for a specific device 
        /// </summary>
        /// <param name="intensity">intensity level ranging from 0..15. </param>
        /// <param name="deviceId">index of cascaded device. </param>
        public void SetBrightness(int intensity, int deviceId)
        {
            if (deviceId < 0 || deviceId >= DeviceCount)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId), $"Invalid device Id {deviceId}");
            }

            if (intensity < 0 || intensity > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(intensity), $"Invalid intensity for Brightness {intensity}");
            }

            SetRegister(deviceId, Register.Intensity, (byte)intensity);
        }

        /// <summary>
        /// Sets the brightness of all cascaded devices to the same intensity level.
        /// </summary>
        /// <param name="intensity">intensity level ranging from 0..15. </param>
        public void SetBrightness(int intensity)
        {
            if (intensity < 0 || intensity > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(intensity), $"Invalid intensity for Brightness {intensity}");
            }
            SetRegister(Register.Intensity, (byte)intensity);
        }

        /// <summary>
        /// Set a number at a specific position
        /// </summary>
        /// <param name="value">the value to display</param>
        /// <param name="digit">the digit index</param>
        /// <param name="deviceId">the cascaded device id</param>
        public void SetDigit(byte value, int digit, int deviceId = 0)
        {
            ValidatePosition(deviceId, digit);
            buffer[deviceId, digit] = value;
        }

        /// <summary>
        /// Get the number at a specific position
        /// </summary>
        /// <param name="digit">the digit index</param>
        /// <param name="deviceId">the cascaded device id</param>
        public int GetDigit(int digit, int deviceId = 0)
        {
            ValidatePosition(deviceId, digit);
            return buffer[deviceId, digit];
        }

        private void ValidatePosition(int deviceId, int digit)
        {
            if (deviceId < 0 || deviceId >= DeviceCount)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId), $"Invalid device Id: {deviceId}");
            }
            if (digit < 0 || digit >= DigitsPerDevice)
            {
                throw new ArgumentOutOfRangeException(nameof(digit), $"Invalid digit: {digit}");
            }
        }

        /// <summary>
        /// Writes all the Values to the devices.
        /// </summary>
        public void Show()
        {
            WriteBuffer(buffer);
        }

        /// <summary>
        /// Writes a two dimensional buffer containing all the values to the devices.
        /// </summary>
        public void WriteBuffer(byte[,] buffer)
        {
            for (int digit = 0; digit < DigitsPerDevice; digit++)
            {
                var i = 0;

                for (int deviceId = DeviceCount - 1; deviceId >= 0; deviceId--)
                {
                    writeBuffer[i++] = (byte)((int)Register.Digit0 + digit);
                    writeBuffer[i++] = buffer[deviceId, digit];
                }
                spiComms.Write(writeBuffer);
            }
        }

        /// <summary>
        /// Clears the buffer from the given start to end (exclusive) and flushes
        /// </summary>
        public void Clear(int start, int end)
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
                for (int digit = 0; digit < DigitsPerDevice; digit++)
                {
                    SetDigit(0, digit, deviceId);
                }
            }
        }
    }
}