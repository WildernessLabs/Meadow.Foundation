using System;
using System.Linq;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class BME280SPI : BME280Comms
    {
        private ISpiBus _spi;
        private IDigitalOutputPort _chipSelect;

        internal BME280SPI(ISpiBus spi, IDigitalOutputPort chipSelect = null)
        {
            _spi = spi;
            _chipSelect = chipSelect;
        }

        public override byte[] ReadRegisters(byte startRegister, int readCount)
        {
            // the buffer needs to be big enough for the output and response
            var buffer = new byte[readCount + 1];
            var bufferTx = new byte[readCount + 1];
            buffer[0] = startRegister;

          //  var rx = _spi.ExchangeData(_chipSelect, buffer);

            var rx = _spi.ReceiveData(_chipSelect, readCount + 1);

            // skip past the byte where we clocked out the register address
            var registerData = rx.Skip(1).Take(readCount).ToArray();

            return registerData;
        }

        public override void WriteRegister(Register register, byte value)
        {
            _spi.SendData(_chipSelect, (byte)register, value);
        }
    }
}