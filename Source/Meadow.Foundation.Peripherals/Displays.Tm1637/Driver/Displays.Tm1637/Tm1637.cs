using System;
using System.Runtime.InteropServices;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Tm1637
{
    /// <summary>
    /// Represents Tm1637 segment display
    /// </summary>
    public sealed class Tm1637
    {
        #region Properties

        /// <summary>
        /// The number of segments that the TM1637 can handle
        /// </summary>
        public byte MAX_SEGMENTS => 6;

        /// <summary>
        /// Order of segments, expect a 6 length byte array
        /// 0 to 5, any order. Most of the time 4 segments do
        /// not need to be changed but the 6 ones may be in different order
        /// like 0 1 2 5 4 3. In this case, this byte array has be be in this order
        /// </summary>
        public byte[] SegmentOrder
        {
            get => segmentOrder; 
            set
            {
                if (value.Length != MAX_SEGMENTS)
                { throw new ArgumentException($"Size of {nameof(SegmentOrder)} can only be 6 length"); }
                // Check if we have all values from 0 to 5

                bool allExist = true;
                for (int i = 0; i < MAX_SEGMENTS; i++)
                {
                    allExist &= Array.Exists(value, e => e == i);
                }
                if (!allExist)
                { throw new ArgumentException($"{nameof(SegmentOrder)} needs to have all existing segments from 0 to 5"); }

                value.CopyTo(segmentOrder, 0);
            }
        }

        /// <summary>
        /// Set the screen on or off
        /// </summary>
        public bool ScreenOn
        {
            get => _screenOn; 

            set
            {
                _screenOn = value;
                DisplayRaw(0, displayData[0]);
            }
        }

        /// <summary>
        /// Adjust the screen brightness from 0 to 7
        /// </summary>
        public byte Brightness
        {
            get => _brightness; 
            set
            {
                if (value > 7)
                { throw new ArgumentException($"{nameof(Brightness)} can't be more than 7"); }

                _brightness = value;
                DisplayRaw(0, displayData[0]);
            }
        }

        #endregion Properties

        #region Member variables / fields

        // According to the doc, the clock pulse width minimum is 400 ns
        // And waiting time between clk up and down is 1 µs
        private const byte ClockWidthMicroseconds = 1;

        private readonly IDigitalOutputPort portClock;
        private readonly IBiDirectionalPort portData;

        private byte _brightness;
        // Default segment order is from 0 to 5
        private byte[] segmentOrder = { 0, 1, 2, 3, 4, 5 };
        // To store what has been displayed last 
        // screen on/off is used
        private byte[] displayData = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        private bool _screenOn;

        #endregion Member variables / fields

        #region Enums

        /// <summary>
        /// Internal registers to be send to the TM1637
        /// </summary>
        internal enum DataCommand
        {
            DataCommandSetting = 0b0100_0000,
            DisplayAndControlCommandSetting = 0b1000_0000,
            AddressCommandSetting = 0b1100_0000,
            ReadKeyScanData = 0b0100_0010,
            FixAddress = 0b0100_0100,
            TestMode = 0b0100_1000,
        }

        /// <summary>
        /// Switch on or off the 8 segments LCD 
        /// </summary>
        internal enum DisplayCommand
        {
            DisplayOn = 0b1000_1000,
            DisplayOff = 0b1000_0000,
        }

        #endregion Enums

        #region Constructors

        private Tm1637() { }

        /// <summary>
        /// Initialize a TM1637
        /// </summary>
        /// <param name="pinClock">The clock pin</param>
        /// <param name="pinData">The data pin</param>
        public Tm1637(IIODevice device, IPin pinClock, IPin pinData)
        {
            portClock = device.CreateDigitalOutputPort(pinClock);
            portData = device.CreateBiDirectionalPort(pinData);

            _brightness = 7;
        }

        private bool WriteByte(byte data)
        {
            for (byte i = 0; i < 8; i++)
            {
                portClock.State = false;
                Thread.Sleep(1); //was waiting for 1 micro second

                // LSB first
                if ((data & 0x01) == 0x01)
                {
                    portData.State = true;
                }
                else
                {
                    portData.State = false;
                }
                    
                // LSB first
                data >>= 1;
                portClock.State = true;
                Thread.Sleep(1); //was waiting for 1 micro second
            }

            // Wait for acknowledge
            portClock.State = false;
            portData.State = true;
            portClock.State = true;
            Thread.Sleep(1); //was waiting for 1 micro second

            portData.Direction = PortDirectionType.Input;
            Thread.Sleep(1); //was waiting for 1 micro second

            var ack = portData.State;

            if (ack == false)
            {
                portData.Direction = PortDirectionType.Output;
                portData.State = false;
            }

            portClock.State = true;
            Thread.Sleep(1); //was waiting for 1 micro second
            portClock.State = false;
            Thread.Sleep(1); //was waiting for 1 micro second

            portData.Direction = PortDirectionType.Output;

            return ack;
        }

        private void StartTransmission()
        {
            portClock.State = true;
            portData.State = true;
            Thread.Sleep(1); //was waiting for 1 micro second
            portData.State = false;
        }

        private void StopTransmission()
        {
            portClock.State = false;
            portData.State = false;
            Thread.Sleep(1); //was waiting for 1 micro second
            portData.State = false;
            portClock.State = true;
            portData.State = true;
        }

        /// <summary>
        /// Displays segments starting at first segment with byte array containing raw data for each segment including the dot
        /// <remarks>
        /// Segment representation:
        /// 
        /// bit 0 = a       _a_
        /// bit 1 = b      |   |
        /// bit 2 = c      f   b
        /// bit 3 = d      |_g_|
        /// bit 4 = e      |   |
        /// bit 5 = f      e   c
        /// bit 6 = g      |_d_|  .dp
        /// bit 7 = dp
        /// 
        /// Representation of the number 0 so lighting segments a, b, c, d, e and F is then 0x3f
        /// </remarks>
        /// </summary>
        /// <param name="rawData">The raw data array to display, size of the array has to be 6 maximum</param>
        private void Display(ReadOnlySpan<byte> rawData)
        {
            if (rawData.Length > MAX_SEGMENTS)
            {
                throw new ArgumentException($"Maximum number of segments for TM1637 is {MAX_SEGMENTS}");
            }

            // Prepare the buffer with the right order to transfer
            var toTransfer = new byte[MAX_SEGMENTS];

            for (int i = 0; i < rawData.Length; i++)
            {
                toTransfer[segmentOrder[i]] = rawData[i];
            }

            for (int j = rawData.Length; j < MAX_SEGMENTS; j++)
            {
                toTransfer[segmentOrder[j]] = (byte)Character.None;
            }
            displayData = toTransfer;

            StartTransmission();
            // First command is set data
            WriteByte((byte)DataCommand.DataCommandSetting);
            StopTransmission();
            StartTransmission();
            // Second command is set address to automatic
            WriteByte((byte)DataCommand.DataCommandSetting);
            // Transfer the data
            for (int i = 0; i < MAX_SEGMENTS; i++)
                WriteByte(toTransfer[i]);

            StopTransmission();
            StartTransmission();
            // Set the display on/off and the brightness
            WriteByte((byte)((ScreenOn ? DisplayCommand.DisplayOn : DisplayCommand.DisplayOff) + _brightness));
            StopTransmission();
        }

        /// <summary>
        /// Displays a series of prebuild characters including the dot or not
        /// You can build yourwwon characters with the primitives like Bottom, Top, Dot
        /// </summary>
        /// <param name="rawData">The Character to display</param>
        public void Display(ReadOnlySpan<Character> rawData)
        {
            Display(MemoryMarshal.AsBytes(rawData));
        }


        /// <summary>
        /// Displays a raw data at a specific segment position from 0 to 5
        /// </summary>
        /// <remarks>
        /// Segment representation:
        /// 
        /// bit 0 = a       _a_
        /// bit 1 = b      |   |
        /// bit 2 = c      f   b
        /// bit 3 = d      |_g_|
        /// bit 4 = e      |   |
        /// bit 5 = f      e   c
        /// bit 6 = g      |_d_|  .dp
        /// bit 7 = dp
        /// 
        /// Representation of the number 0 so lighting segments a, b, c, d, e and F is then 0x3f
        /// </remarks>
        /// <param name="index">The segment position from 0 to 5</param>
        /// <param name="character">The segemnet characters to display</param>
        public void Display(byte index, Character character)
        {
            if (index > MAX_SEGMENTS)
            {
                throw new ArgumentException($"Max segments supported by TM1637 is {MAX_SEGMENTS}");
            }

            // Recreate the buffer in correct order
            displayData[segmentOrder[index]] = (byte)character;

            DisplayRaw(segmentOrder[index], (byte)character);
        }

        private void DisplayRaw(byte segmentAddress, byte rawData)
        {
            StartTransmission();
            // First command for fix address
            WriteByte((byte)DataCommand.FixAddress);
            StopTransmission();
            StartTransmission();
            // Fix address with the address
            WriteByte((byte)(DataCommand.FixAddress + segmentAddress));
            // Transfer the byte
            WriteByte(rawData);
            StopTransmission();
            StartTransmission();
            // Set the display on/off and the brightness
            WriteByte((byte)((ScreenOn ? DisplayCommand.DisplayOn : DisplayCommand.DisplayOff) + _brightness));
            StopTransmission();
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        public void Clear()
        {
            // 6 segments with nothing/space displayed
            Span<byte> clearDisplay = stackalloc byte[]
            {
                (byte)Character.None,
                (byte)Character.None,
                (byte)Character.None,
                (byte)Character.None,
                (byte)Character.None,
                (byte)Character.None,
            };
            Display(clearDisplay);
        }

        #endregion Methods
    }
}