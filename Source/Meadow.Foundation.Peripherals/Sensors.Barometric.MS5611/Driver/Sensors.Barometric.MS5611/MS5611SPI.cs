using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Barometric
{
    internal class MS5611SPI : MS5611Base
    {
        private ISpiBus _spi;
        private IPin _chipSelect;

        internal MS5611SPI(ISpiBus spi, IPin chipSelect, MS5611.Resolution resolution)
            : base(resolution)
        {
            _spi = spi;
            _chipSelect = chipSelect;

            throw new NotImplementedException();
        }

        public override void BeginPressureConversion()
        {
            throw new NotImplementedException();
        }

        public override void BeginTempConversion()
        {
            throw new NotImplementedException();
        }

        public override byte[] ReadData()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
