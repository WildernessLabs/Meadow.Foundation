using System;
using System.Linq;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Bme280Spi : Bme280Comms
    {
        private IDigitalOutputPort _chipSelect;

        ISpiPeripheral spiPeripheral;

        internal Bme280Spi(ISpiBus spi, IDigitalOutputPort? chipSelect = null)
        {
            spiPeripheral = new SpiPeripheral(spi, chipSelect);
        }

        public override void ReadRegisters(byte startRegister, Span<byte> readBuffer)
        {
            spiPeripheral.ReadRegister(startRegister, readBuffer);

            // skip past the byte where we clocked out the register address
            for (int i = 1; i < readBuffer.Length; i++) {
                readBuffer[i - 1] = readBuffer[i];
            }
        }

        public override void WriteRegister(Register register, byte value)
        {
            spiPeripheral.WriteRegister(((byte)register), value);
        }
    }
}