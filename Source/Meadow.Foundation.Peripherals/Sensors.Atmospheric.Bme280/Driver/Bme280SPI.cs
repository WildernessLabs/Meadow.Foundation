using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Bme280Spi : Bme280Comms
    {
        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        internal readonly ISpiCommunications spiComms;

        internal Bme280Spi(ISpiBus spi, Frequency busSpeed, SpiClockConfiguration.Mode busMode, IDigitalOutputPort? chipSelect = null)
        {
            spiComms = new SpiCommunications(spi, chipSelect, busSpeed, busMode);
        }

        public override void ReadRegisters(byte startRegister, Span<byte> readBuffer)
        {
            spiComms.ReadRegister(startRegister, readBuffer);

            // skip past the byte where we clocked out the register address
            for (int i = 1; i < readBuffer.Length; i++)
            {
                readBuffer[i - 1] = readBuffer[i];
            }
        }

        public override void WriteRegister(Register register, byte value)
        {
            spiComms.WriteRegister((byte)register, value);
        }
    }
}