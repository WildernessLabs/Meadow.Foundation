using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    ///     Driver for the Max44009 light-to-digital converter.
    /// </summary>
    public partial class Max44009 : ByteCommsSensorBase<Illuminance>
    {
        public Max44009(II2cBus i2cBus, byte address = Addresses.Low)
            : base (i2cBus, address)
        {
            Initialize();
        }

        protected void Initialize()
        {
            Peripheral.WriteRegister(0x02, 0x00);
        }

        protected override Task<Illuminance> ReadSensor()
        {
            return Task.Run(() => {
                Peripheral.ReadRegister(0x03, ReadBuffer.Span[0..2]);

                int exponent = ((ReadBuffer.Span[0] & 0xF0) >> 4);
                int mantissa = ((ReadBuffer.Span[0] & 0x0F) >> 4) | (ReadBuffer.Span[1] & 0x0F);

                var luminance = Math.Pow(2, exponent) * mantissa * 0.045;

                return new Illuminance(luminance, Illuminance.UnitType.Lux);
            });
        }
    }
}