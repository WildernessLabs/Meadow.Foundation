using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Ms5611Spi : Ms5611Base
    {
        private ISpiBus _spi;
        private IPin _chipSelect;

        internal Ms5611Spi(ISpiBus spi, IPin chipSelect, Ms5611.Resolution resolution)
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
