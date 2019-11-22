using Meadow.Hardware;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.RTCs
{
    public class DS1307
    {
        private const int OriginYear = 1980;

        private II2cBus _bus;

        public byte Address { get; } = 0x68;

        public DS1307(II2cBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// Stops or starts the clock oscillator.  Stopping the oscillator decreases power consumption (and stops the clock)
        /// </summary>
        public bool IsRunning
        {
            get
            {
                // read 1 byte starting from 0x00
                var reg = _bus.WriteReadData(Address, 0x01, 0x00);
                return (reg[0] & (1 << 7)) != 0;
            }
            set
            {
                // read the seconds register
                var reg = _bus.WriteReadData(Address, 0x01, 0x00);
                var current = (reg[0] & (1 << 7)) != 0;
                if ((value && current) || (!value && !current)) return;

                // set the CH bit (bit 7) as appropriate
                if (value)
                {
                    reg[0] |= (1 << 7);
                }
                else
                {
                    reg[0] = (byte)(reg[0] & ~(1 << 7));
                }

                // and write it back to register 0x00
                _bus.WriteData(Address, 0x00, reg[0]);
            }
        }

        public DateTime GetTime()
        {
            // read 7 bytes starting from 0x00
            var data = _bus.WriteReadData(Address, 0x07, 0x00);
            return FromRTCTime(data);
        }

        public void SetTime(DateTime time)
        {
            var data = new List<byte>();
            data.Add(0); // target start register offset
            data.AddRange(ToRTCTime(time));

            _bus.WriteData(Address, data.ToArray());
        }

        /// <summary>
        /// The DS1307 has 56 bytes of battery-backed RAM.  Use this method to Read from that memory.
        /// </summary>
        /// <param name="offset">Offset to the start of the read (0-55)</param>
        /// <param name="count">The number of bytes to read</param>
        /// <returns></returns>
        public byte[] ReadRAM(int offset, int count)
        {
            // RAM starts at register offset 8
            return _bus.WriteReadData(Address, count, (byte)(0x08 + offset));
        }

        /// <summary>
        /// The DS1307 has 56 bytes of battery-backed RAM.  Use this method to Write to that memory
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="data"></param>
        public void WriteRAM(int offset, params byte[] data)
        {
            var d = new List<byte>();
            d.Add((byte)(0x08 + offset)); // target start register offset
            d.AddRange(data);

            _bus.WriteData(Address, d.ToArray());
        }

        public void SquareWaveOutput(SquareWaveFrequency freq)
        {
            byte registerData;

            switch (freq)
            {
                case SquareWaveFrequency.Wave_1000Hz:
                    registerData = (1 << 4);
                    break;
                case SquareWaveFrequency.Wave_4096Hz:
                    registerData = (1 << 4) | (1 << 0);
                    break;
                case SquareWaveFrequency.Wave_8192Hz:
                    registerData = (1 << 4) | (1 << 1);
                    break;
                case SquareWaveFrequency.Wave_32768Hz:
                    registerData = (1 << 4) | (1 << 0) | (1 << 1);
                    break;
                case SquareWaveFrequency.Wave_Low:
                    registerData = 0;
                    break;
                case SquareWaveFrequency.Wave_High:
                    registerData = (1 << 7);
                    break;
                default:
                    throw new NotSupportedException();
            }

            // control register is at 0x07
            _bus.WriteData(Address, 0x07, registerData);
        }

        private static byte ToBCD(ushort i)
        {
            return (byte)((i % 10) + ((i / 10) * 0x10));
        }

        private static ushort FromBCD(byte bcd)
        {
            return (ushort)(((bcd) & 0x0F) + (((bcd) >> 4) * 10));
        }

        private static byte[] ToRTCTime(DateTime dt)
        {
            var data = new byte[7];
            data[0] = ToBCD((ushort)dt.Second);
            data[1] = ToBCD((ushort)dt.Minute);
            data[2] = ToBCD((ushort)dt.Hour);
            data[3] = ToBCD((ushort)((int)dt.DayOfWeek + 1));
            data[4] = ToBCD((ushort)dt.Day);
            data[5] = ToBCD((ushort)dt.Month);
            data[6] = ToBCD((ushort)(dt.Year - OriginYear));
            return data;

        }

        private static DateTime FromRTCTime(byte[] rtcData)
        {
            try
            {
                // is the RTC in 12- or 24-hour mode?
                byte hour = rtcData[2];

                if ((hour & 0x40) != 0)
                {
                    unchecked
                    {
                        // 12-hour mode
                        hour &= (byte)~0x40;
                        if ((hour & 0x20) != 0)
                        {
                            // we're after 12 )PM)
                            hour &= (byte)~0x20;
                            hour += 12;
                        }
                    }
                }

                var y = FromBCD(rtcData[6]) + OriginYear;
                var M = FromBCD(rtcData[5]);
                var d = FromBCD(rtcData[4]);
                var h = FromBCD(hour);
                var m = FromBCD(rtcData[1]);
                var s = FromBCD((byte)(rtcData[0] & 0x7f));

                return new DateTime(
                    y, // year
                    M, // month
                    d, // day
                    h, // hour
                    m, // minute
                    s); // second
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public enum SquareWaveFrequency
        {
            Wave_1000Hz,
            Wave_4096Hz,
            Wave_8192Hz,
            Wave_32768Hz,
            Wave_Low,
            Wave_High
        }
    }
}
