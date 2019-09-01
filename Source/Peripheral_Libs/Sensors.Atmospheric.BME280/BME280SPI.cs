using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class BME280SPI : BME280Comms
    {
        internal BME280SPI(ISpiBus spi)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReadRegisters(byte startRegister, int readCount)
        {
            throw new NotImplementedException();
        }

        public override void WriteRegister(Register register, byte value)
        {
            throw new NotImplementedException();
        }
    }
}