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
            //            _spi.SendData(null, startRegister);
            //            var rx = _spi.ReceiveData(null, readCount);
            //            return rx;

            var buffer = new byte[readCount + 1];
            buffer[0] = startRegister;

            var rx = _spi.ExchangeData(_chipSelect, buffer);

            var registerData = rx.Skip(1).Take(readCount).ToArray();

            Console.WriteLine($" BME Register Data {BitConverter.ToString(registerData)}");

            return registerData;
        }

        public override void WriteRegister(Register register, byte value)
        {
            _spi.SendData(_chipSelect, (byte)register, value);
        }
    }
}