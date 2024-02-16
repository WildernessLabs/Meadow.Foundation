﻿using Meadow.Hardware;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents Tm1637 segment display
    /// </summary>
    public class Tm1637 : IDisposable
    {
        /// <summary>
        /// Max segments for a TM1637 controller
        /// </summary>
        public byte MAX_SEGMENTS => 6;

        /// <summary>
        /// Order of segments
        /// </summary>
        public byte[] SegmentOrder
        {
            get => _segmentOrder;
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
                { throw new ArgumentException($"{nameof(SegmentOrder)} must include all segments (0 to 5)"); }

                value.CopyTo(_segmentOrder, 0);
            }
        }
        private readonly byte[] _segmentOrder = { 0, 1, 2, 3, 4, 5 };

        /// <summary>
        /// Set the screen on or off
        /// </summary>
        public bool ScreenOn
        {
            get => _screenOn;

            set
            {
                _screenOn = value;
                Show(0, displayBuffer[0]);
            }
        }
        private bool _screenOn;

        /// <summary>
        /// Adjust the screen brightness from 0 to 7
        /// </summary>
        public byte Brightness
        {
            get => _brightness;
            set
            {
                if (value > 7)
                {
                    throw new ArgumentException($"{nameof(Brightness)} must be between 0 and 7 inclusive");
                }

                _brightness = value;
                Show(0, displayBuffer[0]);
            }
        }
        private byte _brightness;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPorts = false;

        private readonly IDigitalOutputPort clockPort;
        private readonly IBiDirectionalInterruptPort dataPort;

        private byte[] displayBuffer = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

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

        /// <summary>
        /// Initialize a TM1637
        /// </summary>
        /// <param name="pinClock">The clock pin</param>
        /// <param name="pinData">The data pin</param>
        public Tm1637(IPin pinClock, IPin pinData)
        {
            clockPort = pinClock.CreateDigitalOutputPort();
            dataPort = pinData.CreateBiDirectionalPort();

            createdPorts = true;

            _brightness = 7;
        }

        private bool WriteByte(byte data)
        {
            for (byte i = 0; i < 8; i++)
            {
                clockPort.State = false;
                Thread.Sleep(1);

                if ((data & 0x01) == 0x01)
                {
                    dataPort.State = true;
                }
                else
                {
                    dataPort.State = false;
                }

                data >>= 1;
                clockPort.State = true;
                Thread.Sleep(1);
            }

            clockPort.State = false;
            dataPort.State = true;
            clockPort.State = true;
            Thread.Sleep(1);

            dataPort.Direction = PortDirectionType.Input;
            Thread.Sleep(1);

            var response = dataPort.State;

            if (response == false)
            {
                dataPort.Direction = PortDirectionType.Output;
                dataPort.State = false;
            }

            clockPort.State = true;
            Thread.Sleep(1);
            clockPort.State = false;
            Thread.Sleep(1);

            dataPort.Direction = PortDirectionType.Output;

            return response;
        }

        private void StartTransmission()
        {
            clockPort.State = true;
            dataPort.State = true;
            Thread.Sleep(1);
            dataPort.State = false;
        }

        private void StopTransmission()
        {
            clockPort.State = false;
            dataPort.State = false;
            Thread.Sleep(1);
            dataPort.State = false;
            clockPort.State = true;
            dataPort.State = true;
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
        /// bit 6 = g      |_d_|  .db
        /// bit 7 = dp
        /// 
        /// Representation of the number 0 so lighting segments a, b, c, d, e and F is then 0x3f
        /// </remarks>
        /// </summary>
        /// <param name="data">The raw data array to display, size of the array has to be 6 maximum</param>
        private void Show(ReadOnlySpan<byte> data)
        {
            if (data.Length > MAX_SEGMENTS)
            {
                throw new ArgumentException($"Maximum number of segments for TM1637 is {MAX_SEGMENTS}");
            }

            var toTransfer = new byte[MAX_SEGMENTS];

            for (int i = 0; i < data.Length; i++)
            {
                toTransfer[_segmentOrder[i]] = data[i];
            }

            for (int j = data.Length; j < MAX_SEGMENTS; j++)
            {
                toTransfer[_segmentOrder[j]] = (byte)Character.None;
            }
            displayBuffer = toTransfer;

            StartTransmission();
            WriteByte((byte)DataCommand.DataCommandSetting);
            StopTransmission();
            StartTransmission();
            WriteByte((byte)DataCommand.DataCommandSetting);

            for (int i = 0; i < MAX_SEGMENTS; i++)
            {
                WriteByte(toTransfer[i]);
            }

            StopTransmission();
            StartTransmission();
            WriteByte((byte)((ScreenOn ? DisplayCommand.DisplayOn : DisplayCommand.DisplayOff) + _brightness));
            StopTransmission();
        }

        /// <summary>
        /// Displays a series of prebuilt characters including the dot or not
        /// You can build your own characters with the primitives like Bottom, Top, Dot
        /// </summary>
        /// <param name="data">The Character to display</param>
        public void Show(ReadOnlySpan<Character> data)
        {
            Show(MemoryMarshal.AsBytes(data));
        }

        /// <summary>
        /// Displays a raw data at a specific segment position from 0 to 5
        /// </summary>
        /// <param name="index">The segment position from 0 to 5</param>
        /// <param name="character">The segment characters to display</param>
        public void Show(byte index, Character character)
        {
            if (index > MAX_SEGMENTS)
            {
                throw new ArgumentException($"Max segments supported by TM1637 is {MAX_SEGMENTS}");
            }

            displayBuffer[_segmentOrder[index]] = (byte)character;

            Show(_segmentOrder[index], (byte)character);
        }

        private void Show(byte segmentAddress, byte show)
        {
            StartTransmission();
            WriteByte((byte)DataCommand.FixAddress);
            StopTransmission();
            StartTransmission();
            WriteByte((byte)(DataCommand.FixAddress + segmentAddress));
            WriteByte(show);
            StopTransmission();
            StartTransmission();
            WriteByte((byte)((ScreenOn ? DisplayCommand.DisplayOn : DisplayCommand.DisplayOff) + _brightness));
            StopTransmission();
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        public void Clear()
        {
            Span<byte> clearDisplay = stackalloc byte[]
            {
                (byte)Character.None,
                (byte)Character.None,
                (byte)Character.None,
                (byte)Character.None,
                (byte)Character.None,
                (byte)Character.None,
            };
            Show(clearDisplay);
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPorts)
                {
                    clockPort.Dispose();
                    dataPort.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}