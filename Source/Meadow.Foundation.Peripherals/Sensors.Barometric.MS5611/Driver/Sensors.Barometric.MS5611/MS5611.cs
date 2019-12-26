using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Barometric
{
    public class MS5611
    {
        private MS5611Base _impl;

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
        public MS5611(II2cBus i2c, byte address = 0x76, Resolution resolution = Resolution.OSR_1024)
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
            
            _impl = new MS5611I2C(i2c, address, resolution);
        }

        /// <summary>
        /// Connect to the GY63 using SPI (PS must be pulled low)
        /// </summary>
        /// <param name="spi"></param>
        /// <param name="resolution"></param>
        public MS5611(ISpiBus spi, IPin chipSelect, Resolution resolution = Resolution.OSR_1024)
        {
            _impl = new MS5611SPI(spi, chipSelect, resolution);
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
