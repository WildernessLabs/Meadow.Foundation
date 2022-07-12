using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Ms5611I2c : Ms5611Base
    {
        private I2cPeripheral i2CPeripheral;

        internal Ms5611I2c(II2cBus i2cBus, byte address, Ms5611.Resolution resolution)
            : base(resolution)
        {
            i2CPeripheral = new I2cPeripheral(i2cBus, address);
        }

        public override void Reset()
        {
            var cmd = (byte)Commands.Reset;

            i2CPeripheral.Write(cmd);
        }

        public override void BeginTempConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD2 + 2 * (byte)Resolution);
            i2CPeripheral.Write(cmd);
        }

        public override void BeginPressureConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD1 + 2 * (byte)Resolution);
            i2CPeripheral.Write(cmd);
        }

        public override byte[] ReadData()
        {
            var data = new byte[3];
            i2CPeripheral.ReadRegister((byte)Commands.ReadADC, data);
            return data;
        }
    }
}