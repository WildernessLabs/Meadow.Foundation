using System;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Max7219 LED matrix driver
    /// </summary>
    public class Max7219 : DisplayBase
    {
        

        /// <summary>
        /// MAX7219 Spi Clock Frequency
        /// </summary>
        public static int SpiClockFrequency => 10000000;

        /// <summary>
        /// Number of digits Register per Module
        /// </summary>
        public const int NumDigits = 8;

        /// <summary>
        /// Number of cascaded devices
        /// </summary>
        /// <value></value>
        public int DeviceCount => DeviceRows * DeviceColumns;
        public int DeviceRows { get; private set; }
        public int DeviceColumns { get; private set; }

        /// <summary>
        /// Gets the total number of digits (cascaded devices * num digits)
        /// </summary>
        public int Length => DeviceCount * NumDigits;

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override int Width => 8 * DeviceColumns;

        public override int Height => 8 * DeviceRows;

        private ISpiPeripheral max7219;

        /// <summary>
        /// internal buffer used to write to registers for all devices.
        /// </summary>
        private readonly byte[] writeBuffer;
        private readonly byte[] readBuffer;

        private readonly SpiBus spi;
        private readonly IDigitalOutputPort chipSelectPort;

        /// <summary>
        /// A Buffer that contains the values of the digits registers per device
        /// </summary>
        private readonly byte[,] buffer;

        private Color currentPen;

        private readonly byte DECIMAL = 0b10000000;

        

        

        public enum Max7219Type
        {
            Character, 
            Digital,
            Display, 
        }

        public enum CharacterType : byte
        {
            _0     = 0x00,
            _1     = 0x01,
            _2     = 0x02,
            _3     = 0x03,
            _4     = 0x04,
            _5     = 0x05,
            _6     = 0x06,
            _7     = 0x07,
            _8     = 0x08,
            _9     = 0x09,
            Hyphen = 0x0A,
            E      = 0x0B,
            H      = 0x0C,
            L      = 0x0D,
            P      = 0x0E,
            Blank  = 0x0F,
            count

        }

        internal enum Register : byte
        {
            NoOp        = 0x00,
            Digit0      = 0x01,
            Digit1      = 0x02,
            Digit2      = 0x03,
            Digit3      = 0x04,
            Digit4      = 0x05,
            Digit5      = 0x06,
            Digit6      = 0x07,
            Digit7      = 0x08,
            DecodeMode  = 0x09,
            Intensity   = 0x0A,
            ScanLimit   = 0x0B,
            ShutDown    = 0x0C,
            DisplayTest = 0x0F
        }

        

        

        public Max7219(ISpiBus spiBus, IDigitalOutputPort csPort, int deviceCount = 1, Max7219Type maxMode = Max7219Type.Display)
            :this(spiBus, csPort, 8, 1, maxMode)
        {
        }

        public Max7219(ISpiBus spiBus, IDigitalOutputPort csPort, int deviceRows, int deviceColumns, Max7219Type maxMode = Max7219Type.Display)
        {
            spi = (SpiBus)spiBus;
            chipSelectPort = csPort;

            max7219 = new SpiPeripheral(spiBus, csPort);

            DeviceRows = deviceRows;
            DeviceColumns = deviceColumns;

            buffer = new byte[DeviceCount, NumDigits];
            writeBuffer = new byte[2 * DeviceCount];
            readBuffer = new byte[2 * DeviceCount];

            Initialize(maxMode);
        }

        /// <summary>
        /// Creates a Max7219 Device given a <see paramref="spiBus" /> to communicate over and the
        /// number of devices that are cascaded.
        /// </summary>
        public Max7219(IIODevice device, ISpiBus spiBus, IPin csPin, int deviceRows = 1, int deviceColumns = 1, Max7219Type maxMode = Max7219Type.Display)
            : this(spiBus, device.CreateDigitalOutputPort(csPin), deviceRows, deviceColumns, maxMode)
        { }

        public Max7219(IIODevice device, ISpiBus spiBus, IPin csPin, int deviceCount = 1, Max7219Type maxMode = Max7219Type.Display)
            : this(spiBus, device.CreateDigitalOutputPort(csPin), deviceCount, 1, maxMode)
        { }

        

        /// <summary>
        /// Standard initialization routine.
        /// </summary>
        void Initialize(Max7219Type maxMode)
        {
            SetRegister(Register.DecodeMode, (byte)((maxMode == Max7219Type.Character)? 0xFF:0)); // use matrix(0) or digits
            SetRegister(Register.ScanLimit, 7); //show all 8 digits
            SetRegister(Register.DisplayTest, 0); // no display test
            SetRegister(Register.ShutDown, 1); // not shutdown mode
            SetBrightness(4); //intensity, range: 0..15
            Clear();
        }

        public void SetNumber(int value, int deviceId = 0)
        {
            //12345678
            if(value > 999999999)
            {
                throw new ArgumentOutOfRangeException();
            }

            for(int i = 0; i < 8; i++)
            {
                SetCharacter((CharacterType)(value % 10), i, false, deviceId);
                value /= 10;

                if(value == 0)
                {
                    break;
                }
            }
        }

        public void SetCharacter(CharacterType character, int digit, bool showDecimal = false, int deviceId = 0)
        {
            ValidatePosition(deviceId, digit);
            var data = (byte)((byte)character + (showDecimal ? DECIMAL : 0));

            buffer[deviceId, digit] = data;
        }

        public CharacterType GetCharacter(int digit, int deviceId = 0)
        {
            ValidatePosition(deviceId, digit);
            return (CharacterType)buffer[deviceId, digit];
        }

        public void TestDisplay(int timeInMs = 1000)
        {
            SetRegister(Register.DisplayTest, 0xFF);

            Thread.Sleep(timeInMs);

            SetRegister(Register.DisplayTest, 0); 
        }

        public void SetMode(Max7219Type maxMode)
        {
            SetRegister(Register.DecodeMode, (byte)((maxMode == Max7219Type.Character) ? 0xFF : 0)); // use matrix(0) or digits
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
            max7219.WriteBytes(writeBuffer);
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

        public void SetDigit(byte value, int digit, int deviceId = 0)
        {
            ValidatePosition(deviceId, digit);
            buffer[deviceId, digit] = value;
        }

        public byte GetDigit(int digit, int deviceId = 0)
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
            if (digit < 0 || digit >= NumDigits)
            {
                throw new ArgumentOutOfRangeException(nameof(digit), $"Invalid digit: {digit}");
            }
        }

        /// <summary>
        /// Writes all the Values to the devices.
        /// </summary>
        public override void Show()
        {
            WriteBuffer(buffer);
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
                    writeBuffer[i++] = (byte)((int)Register.Digit0 + digit);
                    writeBuffer[i++] = buffer[deviceId, digit];
                }
                //max7219.WriteBytes(_writeBuffer);
                spi.ExchangeData(chipSelectPort, ChipSelectMode.ActiveLow, writeBuffer, readBuffer);
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
                for (int digit = 0; digit < NumDigits; digit++)
                {
                    SetDigit(0, digit, deviceId);
                }
            }
        }

        /// <summary>
        /// Clears the buffer from the given start to end and flushes
        /// </summary>
        public override void Clear(bool updateDisplay = false)
        {
            Clear(0, DeviceCount);

            if(updateDisplay)
            {
                Show();
            }
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color != Color.Black);
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
            var index = x % 8;

            var display = y / 8 + (x / 8) * DeviceRows;

            if(display > DeviceCount)
            {
                Console.WriteLine($"Display out of range {x}, {y}");
                return;
            }

            if (colored)
            {
                buffer[display, index] = (byte)(buffer[display, index] | (byte)(1 << (y % 8)));
            }
            else
            {
                buffer[display, index] = (byte)(buffer[display, index] & ~(byte)(1 << (y % 8)));
            }
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, currentPen);
        }

        public override void SetPenColor(Color pen)
        {
            currentPen = pen;
        }

        public override void InvertPixel(int x, int y)
        {
            var index = x % 8;

            var display = y / 8 + (x / 8) * DeviceRows;

            if (display > DeviceCount)
            {
                Console.WriteLine($"Display out of range {x}, {y}");
                return;
            }

            buffer[display, index] = (byte)(buffer[display, index] ^= (byte)(1 << y % 8));
        }
    }
}