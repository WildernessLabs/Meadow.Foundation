using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Ms5611I2c : Ms5611Base
    {
        private II2cBus _i2c;
        private byte _address;

        internal Ms5611I2c(II2cBus i2c, byte address, Ms5611.Resolution resolution)
            : base(resolution)
        {
            _i2c = i2c;
            _address = address;
        }

        public override void Reset()
        {
            var cmd = (byte)Commands.Reset;
            Console.WriteLine($"Sending {cmd:X2} to {_address:X2}");
            _i2c.WriteData(_address, cmd);
        }

        public override void BeginTempConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD2 + 2 * (byte)Resolution);
            Console.WriteLine($"Sending {cmd:X2} to {_address:X2}");
            _i2c.WriteData(_address, cmd);
        }

        public override void BeginPressureConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD1 + 2 * (byte)Resolution);
            Console.WriteLine($"Sending {cmd:X2} to {_address:X2}");
            _i2c.WriteData(_address, cmd);
        }

        public override byte[] ReadData()
        {
            // write a
            _i2c.WriteData(_address, (byte)Commands.ReadADC);
            var data = _i2c.ReadData(_address, 3);
            return data;
        }
    }
}
