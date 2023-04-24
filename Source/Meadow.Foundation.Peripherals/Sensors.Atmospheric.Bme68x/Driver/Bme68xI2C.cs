using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Bme68xI2C : Bme68xComms
    {
        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        internal Bme68xI2C(II2cBus i2c, byte busAddress)
        {
            i2cComms = new I2cCommunications(i2c, busAddress);
        }

        public override byte ReadRegister(byte address) => i2cComms.ReadRegister(address);

        public override ushort ReadRegisterAsUShort(byte address, ByteOrder order = ByteOrder.LittleEndian) => i2cComms.ReadRegisterAsUShort(address, order);

        public override void ReadRegister(byte address, Span<byte> readBuffer)
        {
            i2cComms.ReadRegister(address, readBuffer);
        }

        public override void WriteRegister(byte register, byte value)
        {
            i2cComms.WriteRegister(register, value);
        }
    }
}