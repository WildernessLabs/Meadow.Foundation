using Meadow.Hardware;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.RTCs
{
    /// <summary>
    /// Represents a DS1307 real-time clock
    /// </summary>
    public partial class Ds1307 : II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        const int OriginYear = 1980;

        readonly II2cBus i2cBus;

        /// <summary>
        /// Create a new Ds1307 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        public Ds1307(II2cBus i2cBus)
        {
            this.i2cBus = i2cBus;
        }

        /// <summary>
        /// Stops or starts the clock oscillator
        /// Stopping the oscillator decreases power consumption (and stops the clock)
        /// </summary>
        public bool IsRunning
        {
            get
            {   // read 1 byte starting from 0x00
                var reg = new byte[1];
                i2cBus.Write((byte)Addresses.Default, new byte[] { 0 });
                i2cBus.Read((byte)Addresses.Default, reg);
                return (reg[0] & (1 << 7)) != 0;
            }
            set
            {   // read the seconds register
                var reg = new byte[1];
                i2cBus.Write((byte)Addresses.Default, new byte[] { 0 });
                i2cBus.Read((byte)Addresses.Default, reg);
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
                i2cBus.Write((byte)Addresses.Default, reg);
            }
        }

        /// <summary>
        /// Get the time from the real-time clock
        /// </summary>
        /// <returns></returns>
        public DateTime GetTime()
        {
            var data = new byte[7];
            i2cBus.Write((byte)Addresses.Default, new byte[] { 0 });
            i2cBus.Read((byte)Addresses.Default, data);
            return FromRTCTime(data);
        }

        /// <summary>
        /// Set the time on the real-time clock
        /// </summary>
        /// <param name="time">The new time</param>
        public void SetTime(DateTime time)
        {
            var data = new List<byte> { 0 };
            data.AddRange(ToRTCTime(time));

            i2cBus.Write((byte)Addresses.Default, data.ToArray());
        }

        /// <summary>
        /// The DS1307 has 56 bytes of battery-backed RAM.  Use this method to Read from that memory.
        /// </summary>
        /// <param name="offset">Offset to the start of the read (0-55)</param>
        /// <param name="count">The number of bytes to read</param>
        public byte[] ReadRAM(int offset, int count)
        {
            var data = new byte[count];
            i2cBus.Write((byte)Addresses.Default, new byte[] { (byte)(0x08 + offset) });
            i2cBus.Read((byte)Addresses.Default, data);
            return data;
        }

        /// <summary>
        /// The DS1307 has 56 bytes of battery-backed RAM
        /// Use this method to Write to that memory
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="data"></param>
        public void WriteRAM(int offset, params byte[] data)
        {
            var d = new List<byte>
            {
                (byte)(0x08 + offset) // target start register offset
            };
            d.AddRange(data);

            i2cBus.Write((byte)Addresses.Default, d.ToArray());
        }

        /// <summary>
        /// Square wave output
        /// </summary>
        /// <param name="freq">The frequency</param>
        /// <exception cref="NotSupportedException">Throw if invalid frequency is selected</exception>
        public void SquareWaveOutput(SquareWaveFrequency freq)
        {
            var registerData = freq switch
            {
                SquareWaveFrequency.Wave_1000Hz => (byte)(1 << 4),
                SquareWaveFrequency.Wave_4096Hz => (byte)((1 << 4) | (1 << 0)),
                SquareWaveFrequency.Wave_8192Hz => (byte)((1 << 4) | (1 << 1)),
                SquareWaveFrequency.Wave_32768Hz => (byte)((1 << 4) | (1 << 0) | (1 << 1)),
                SquareWaveFrequency.Wave_Low => (byte)0,
                SquareWaveFrequency.Wave_High => (byte)(1 << 7),
                _ => throw new NotSupportedException(),
            };

            // control register is at 0x07
            i2cBus.Write((byte)Addresses.Default, new byte[] { 0x07, registerData }); //register and value
        }

        static byte ToBCD(ushort i)
        {
            return (byte)((i % 10) + ((i / 10) * 0x10));
        }

        static ushort FromBCD(byte bcd)
        {
            return (ushort)(((bcd) & 0x0F) + (((bcd) >> 4) * 10));
        }

        static byte[] ToRTCTime(DateTime dt)
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

        static DateTime FromRTCTime(byte[] rtcData)
        {
            try
            {   // is the RTC in 12- or 24-hour mode?
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
    }
}