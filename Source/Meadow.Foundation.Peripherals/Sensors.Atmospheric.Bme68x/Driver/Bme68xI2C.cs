using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Bme68xI2C : Bme680Comms
    {
        protected I2cPeripheral i2CPeripheral;

        internal Bme68xI2C(II2cBus i2c, byte busAddress)
        {
            i2CPeripheral = new I2cPeripheral(i2c, busAddress);
        }
                    
        public override byte ReadRegister(byte address) => i2CPeripheral.ReadRegister(address);

        public override ushort ReadRegisterAsUShort(byte address, ByteOrder order = ByteOrder.LittleEndian) => i2CPeripheral.ReadRegisterAsUShort(address, order);

        public override void ReadRegister(byte address, Span<byte> readBuffer)
        {
            i2CPeripheral.ReadRegister(address, readBuffer);
        }

        public override void WriteRegister(byte register, byte value)
        {
            i2CPeripheral.WriteRegister(register, value);
        }
    }
}