using System;
using System.Threading;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Graphics;
using Meadow.Units;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Max7219 LED matrix driver
    /// </summary>
    public partial class Max7219 : IGraphicsDisplay
    {
        /// <summary>
        /// MAX7219 Spi Clock Frequency
        /// </summary>
        public static Frequency DefaultSpiBusSpeed = new Frequency(10000000, Frequency.UnitType.Kilohertz);

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

        public ColorType ColorMode => ColorType.Format1bpp;

        public int Width => 8 * DeviceColumns;

        public int Height => 8 * DeviceRows;

        public bool IgnoreOutOfBoundsPixels { get; set; }

        private ISpiPeripheral max7219;

        /// <summary>
        /// internal buffer used to write to registers for all devices.
        /// </summary>
        private readonly byte[] writeBuffer;

        private readonly IDigitalOutputPort chipSelectPort;

        /// <summary>
        /// A Buffer that contains the values of the digits registers per device
        /// </summary>
        private readonly byte[,] buffer;
        
        private readonly byte DECIMAL = 0b10000000;

        public Max7219(ISpiBus spiBus, IDigitalOutputPort csPort, int deviceCount = 1, Max7219Type maxMode = Max7219Type.Display)
            :this(spiBus, csPort, 8, 1, maxMode)
        {
        }

        public Max7219(ISpiBus spiBus, IDigitalOutputPort csPort, int deviceRows, int deviceColumns, Max7219Type maxMode = Max7219Type.Display)
        {
            chipSelectPort = csPort;

            max7219 = new SpiPeripheral(spiBus, csPort);

            DeviceRows = deviceRows;
            DeviceColumns = deviceColumns;

            buffer = new byte[DeviceCount, NumDigits];

            writeBuffer = new byte[2 * DeviceCount];

            Initialize(maxMode);
        }

        /// <summary>
        /// Creates a Max7219 Device given a <see paramref="spiBus" /> to communicate over and the
        /// number of devices that are cascaded.
        /// </summary>
        public Max7219(IMeadowDevice device, ISpiBus spiBus, IPin csPin, int deviceRows = 1, int deviceColumns = 1, Max7219Type maxMode = Max7219Type.Display)
            : this(spiBus, device.CreateDigitalOutputPort(csPin), deviceRows, deviceColumns, maxMode)
        { }

        public Max7219(IMeadowDevice device, ISpiBus spiBus, IPin csPin, int deviceCount = 1, Max7219Type maxMode = Max7219Type.Display)
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
            max7219.Write(writeBuffer);
        }

        /// <summary>
        /// Sends data to a specific register for a specific device
        /// </summary>
        internal void SetRegister(int deviceId, Register register, byte data)
        {
            Array.Clear(writeBuffer, 0, writeBuffer.Length);

            writeBuffer[deviceId * 2] = (byte)register;
            writeBuffer[deviceId * 2 + 1] = data;

            max7219.Write(writeBuffer);
        }

        /// <summary>
        /// Sets the brightness for a specific device 
        /// </summary>
        /// <param name="intensity">intensity level ranging from 0..15. </param>
        /// <param name="deviceId">index of cascaded device. </param>
        public void SetBrightness(int intensity, int deviceId)
        {
            if(deviceId < 0 || deviceId >= DeviceCount)
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
        public void Show()
        {
            WriteBuffer(buffer);
        }

        public void Show(int left, int top, int right, int bottom)
        {
            //ToDo Check if partial updates are possible (although it's pretty fast as is)
            Show();
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
                max7219.Write(writeBuffer);
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
        public void Clear(bool updateDisplay = false)
        {
            Clear(0, DeviceCount);

            if(updateDisplay)
            {
                Show();
            }
        }

        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        public void DrawPixel(int x, int y, bool colored)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

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

        public void InvertPixel(int x, int y)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            var index = x % 8;

            var display = y / 8 + (x / 8) * DeviceRows;

            if (display > DeviceCount)
            {
                Console.WriteLine($"Display out of range {x}, {y}");
                return;
            }

            buffer[display, index] = (buffer[display, index] ^= (byte)(1 << y % 8));
        }

        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            Fill(0, 0, Width, Height, fillColor);

            if (updateDisplay) Show();
        }

        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x > width - 1) x = width - 1;
                if (y > height - 1) y = height - 1;
            }

            bool isColored = fillColor.Color1bpp;
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    DrawPixel(i + x, j + y, isColored);
                }
            }
        }

        public void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {   //need to refactor to use a proper buffer
            for (int i = 0; i < displayBuffer.Width; i++)
            {
                for (int j = 0; j < displayBuffer.Height; j++)
                {
                    DrawPixel(x + i, j + y, displayBuffer.GetPixel(i, j).Color1bpp);
                }
            }
        }
    }
}