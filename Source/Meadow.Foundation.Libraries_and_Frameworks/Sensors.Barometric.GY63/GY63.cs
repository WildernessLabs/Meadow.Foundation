using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Barometric
{
    internal abstract class GY63Base
    {
        protected GY63.Resolution Resolution { get; set; }

        public abstract void Reset();
        public abstract void BeginTempConversion();
        public abstract void BeginPressureConversion();
        public abstract byte[] ReadData();

        protected enum Commands : byte
        {
            Reset = 0x1e,
            ConvertD1 = 0x40,
            ConvertD2 = 0x50,
            ReadADC = 0x00,
        }

        internal GY63Base(GY63.Resolution resolution)
        {
            Resolution = resolution;
        }
    }

    internal class GY63I2C : GY63Base
    {
        private II2cBus _i2c;
        private byte _address;

        internal GY63I2C(II2cBus i2c, byte address, GY63.Resolution resolution)
            : base(resolution)
        {
            _i2c = i2c;
            _address = address;
        }

        public override void Reset()
        {
            var cmd = (byte)Commands.Reset;
            Console.WriteLine($"Sending {cmd:X2} to {_address:X2}");
            _i2c.WriteData(_address, cmd);
        }

        public override void BeginTempConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD2 + 2 * (byte)Resolution);
            Console.WriteLine($"Sending {cmd:X2} to {_address:X2}");
            _i2c.WriteData(_address, cmd);
        }

        public override void BeginPressureConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD1 + 2 * (byte)Resolution);
            Console.WriteLine($"Sending {cmd:X2} to {_address:X2}");
            _i2c.WriteData(_address, cmd);
        }

        public override byte[] ReadData()
        {
            // write a
            _i2c.WriteData(_address, (byte)Commands.ReadADC);
            var data = _i2c.ReadData(_address, 3);
            return data;
        }
    }

    public class GY63
    {
        private GY63Base _impl;

        public enum Resolution
        {
            OSR_256 = 0,
            OSR_412 = 1,
            OSR_1024 = 2,
            OSR_2048 = 3,
            OSR_4096 = 4
        }

        /// <summary>
        /// Connect to the GY63 using I2C (PS must be pulled high)
        /// </summary>
        /// <param name="i2c"></param>
        /// <param name="address">0x76 is CSB is pulled low, 0x77 if CSB is pulled high</param>
        /// <param name="resolution"></param>
        public GY63(II2cBus i2c, byte address = 0x76, Resolution resolution = Resolution.OSR_1024)
        {
            switch (address)
            {
                case 0x76:
                case 0x77:
                    // valid address is either 0x76 or 0x77
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Address must be 0x76 or 0x77");
            }
            
            _impl = new GY63I2C(i2c, address, resolution);
        }

        /// <summary>
        /// Connect to the GY63 using SPI (PS must be pulled low)
        /// </summary>
        /// <param name="spi"></param>
        /// <param name="resolution"></param>
        public GY63(ISpiBus spi, Resolution resolution = Resolution.OSR_1024)
        {
        }

        public void Reset()
        {
            _impl.Reset();
        }

        private void BeginTempConversion()
        {
            _impl.BeginTempConversion();
        }

        private void BeginPressureConversion()
        {
            _impl.BeginPressureConversion();
        }

        private byte[] ReadData()
        {
            return _impl.ReadData();
        }

        public int ReadTemperature()
        {
            _impl.BeginTempConversion();
            Thread.Sleep(10); // 1 + 2 * Resolution

            // we get back 24 bits (3 bytes), regardless of the resolution we're asking for
            var data = _impl.ReadData();

            var result = data[2] | data[1] << 8 | data[0] << 16;

            return result;
        }

        public int ReadPressure()
        {
            _impl.BeginPressureConversion();

            Thread.Sleep(10); // 1 + 2 * Resolution

            // we get back 24 bits (3 bytes), regardless of the resolution we're asking for
            var data = _impl.ReadData();

            var result = data[2] | data[1] << 8 | data[0] << 16;

            return result;
        }
    }
}
