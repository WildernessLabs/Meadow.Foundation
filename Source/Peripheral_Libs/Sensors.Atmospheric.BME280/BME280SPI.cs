using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class BME280SPI : BME280Comms
    {
        private ISpiBus _spi;
        private IPin _chipSelect;

        internal BME280SPI(ISpiBus spi, IPin chipSelect = null)
        {
            _spi = spi;
            _chipSelect = chipSelect;
        }

        public override byte[] ReadRegisters(byte startRegister, int readCount)
        {
            var buffer = new byte[readCount + 1];
            buffer[0] = startRegister;

            var rx = _spi.ExchangeData(_chipSelect, buffer);

            // probably need to return rx[1] on
            return rx;
        }

        public override void WriteRegister(Register register, byte value)
        {
            _spi.SendData(_chipSelect, (byte)register, value);
        }
    }
}