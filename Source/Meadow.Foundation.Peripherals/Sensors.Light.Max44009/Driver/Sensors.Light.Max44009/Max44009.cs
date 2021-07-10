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
                // from the datasheet:
                //  Bits in Lux High-Byte register 0x03 give the 4 bits of
                //  exponent E3:E0 and 4 most significant bits of the mantissa
                //  byte M7:M4, and represent the lux reading of ambient light.
                //  The remaining 4 bits of the mantissa byte M3:M0 are in the
                //  Lux Low - Byte register 0x04 and enhance resolution of the
                //  lux reading from the IC.
                //
                //  Exponent(E[3:0]): Exponent bits of the lux reading(0000 to
                //  1110).Note: A reading of 1111 represents an overrange
                //  condition.
                //
                //  Mantissa(M[7:4]): Four most significant bits of mantissa
                //  byte of the lux reading
                //    (0000 to 1111).Lux = 2(exponent) x mantissa x 0.72
                //  Exponent = 8xE3 + 4xE2 + 2xE1 + E0
                //  Mantissa = 8xM7 + 4xM6 + 2xM5 + M4
                //  A code of 0000 0001 calculates to be 0.72 lux.
                //  A code of 1110 1111 calculates to be 176,947 lux.
                //  A code of 1110 1110 calculates to be 165,151 lux.
                Peripheral.ReadRegister(0x03, ReadBuffer.Span[0..2]);

                //int exponent = ((ReadBuffer.Span[0] & 0xF0) >> 4);
                var exponent = (ReadBuffer.Span[0] >> 4);
                if (exponent == 0x0f) throw new Exception("Out of range");

                int mantissa = ((ReadBuffer.Span[0] & 0x0F) >> 4) | (ReadBuffer.Span[1] & 0x0F);

                //var luminance = Math.Pow(2, exponent) * mantissa * 0.045;
                var luminance = Math.Pow(2, exponent) * mantissa * 0.72;

                return new Illuminance(luminance, Illuminance.UnitType.Lux);
            });
        }
    }
}