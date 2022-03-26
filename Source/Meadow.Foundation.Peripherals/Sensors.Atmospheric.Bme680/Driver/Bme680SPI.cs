using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Bme680SPI : Bme680Comms
    {
        ISpiPeripheral spiPeripheral;

        public override byte ReadRegister(byte register) => spiPeripheral.ReadRegister(register);

        internal Bme680SPI(ISpiBus spi, IDigitalOutputPort? chipSelect = null)
        {
            spiPeripheral = new SpiPeripheral(spi, chipSelect);
        }

        public override void ReadRegisters(byte startRegister, Span<byte> readBuffer)
        {
            spiPeripheral.ReadRegister(startRegister, readBuffer);
            /*
            // skip past the byte where we clocked out the register address
            for (int i = 1; i < readBuffer.Length; i++) 
            {
                readBuffer[i - 1] = readBuffer[i];
            }*/
        }

        public override void WriteRegister(byte register, byte value)
        {
            spiPeripheral.WriteRegister(register, value);
        }
    }
}