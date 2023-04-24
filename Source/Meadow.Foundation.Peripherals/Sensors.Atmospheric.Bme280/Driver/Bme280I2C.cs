using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Bme280I2C : Bme280Comms
    {
        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        internal Bme280I2C(II2cBus i2c, byte busAddress)
        {
            if ((busAddress != 0x76) && (busAddress != 0x77))
            {
                throw new ArgumentOutOfRangeException(nameof(busAddress), "Address should be 0x76 or 0x77");
            }
            i2cComms = new I2cCommunications(i2c, busAddress);
        }

        public override void ReadRegisters(byte startRegister, Span<byte> readBuffer)
        {
            i2cComms.ReadRegister(startRegister, readBuffer);
        }

        public override void WriteRegister(Register register, byte value)
        {
            i2cComms.WriteRegister((byte)register, value);
        }
    }
}