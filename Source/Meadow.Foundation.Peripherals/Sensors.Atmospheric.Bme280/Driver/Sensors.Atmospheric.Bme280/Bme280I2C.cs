using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Bme280I2C : Bme280Comms
    {
        protected I2cPeripheral i2CPeripheral;

        internal Bme280I2C(II2cBus i2c, byte busAddress)
        {
            if ((busAddress != 0x76) && (busAddress != 0x77))
            {
                throw new ArgumentOutOfRangeException(nameof(busAddress), "Address should be 0x76 or 0x77");
            }
            i2CPeripheral = new I2cPeripheral(i2c, busAddress);
        }

        public override void ReadRegisters(byte startRegister, Span<byte> readBuffer)
        {
            i2CPeripheral.ReadRegister(startRegister, readBuffer);
        }

        public override void WriteRegister(Register register, byte value)
        {
            i2CPeripheral.WriteRegister(((byte)register), value);
        }
    }
}