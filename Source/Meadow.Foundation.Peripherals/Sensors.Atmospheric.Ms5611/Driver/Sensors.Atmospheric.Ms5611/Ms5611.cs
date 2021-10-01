using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Ms5611
    {
        private Ms5611Base ms5611;

        /// <summary>
        /// Connect to the Ms5611 using I2C
        /// </summary>
        /// <param name="i2c"></param>
        /// <param name="address">i2c address - default is 0x5c</param>
        /// <param name="resolution"></param>
        public Ms5611(II2cBus i2c, byte address = (byte)Addresses.Default, Resolution resolution = Resolution.OSR_1024)
        {
            ms5611 = new Ms5611I2c(i2c, address, resolution);
        }

        /// <summary>
        /// Connect to the Ms5611 using SPI (PS must be pulled low)
        /// </summary>
        /// <param name="spi"></param>
        /// <param name="resolution"></param>
        public Ms5611(ISpiBus spi, IPin chipSelect, Resolution resolution = Resolution.OSR_1024)
        {
            ms5611 = new Ms5611Spi(spi, chipSelect, resolution);
        }

        public void Reset()
        {
            ms5611.Reset();
        }

        private void BeginTempConversion()
        {
            ms5611.BeginTempConversion();
        }

        private void BeginPressureConversion()
        {
            ms5611.BeginPressureConversion();
        }

        private byte[] ReadData()
        {
            return ms5611.ReadData();
        }

        public int ReadTemperature()
        {
            ms5611.BeginTempConversion();
            Thread.Sleep(10); // 1 + 2 * Resolution

            // we get back 24 bits (3 bytes), regardless of the resolution we're asking for
            var data = ms5611.ReadData();

            var result = data[2] | data[1] << 8 | data[0] << 16;

            return result;
        }

        public int ReadPressure()
        {
            ms5611.BeginPressureConversion();

            Thread.Sleep(10); // 1 + 2 * Resolution

            // we get back 24 bits (3 bytes), regardless of the resolution we're asking for
            var data = ms5611.ReadData();

            var result = data[2] | data[1] << 8 | data[0] << 16;

            return result;
        }
    }
}
