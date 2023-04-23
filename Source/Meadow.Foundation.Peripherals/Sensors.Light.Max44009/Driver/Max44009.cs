using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Driver for the Max44009 light-to-digital converter
    /// </summary>
    public partial class Max44009 : ByteCommsSensorBase<Illuminance>
    {
        /// <summary>
        /// Create a new Max44009 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Max44009(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
            Initialize();
        }

        /// <summary>
        /// Initalize the sensor
        /// </summary>
        protected void Initialize()
        {
            Peripheral.WriteRegister(0x02, 0x00);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<Illuminance> ReadSensor()
        {
            Peripheral.ReadRegister(0x03, ReadBuffer.Span[0..2]);

            var exponent = (ReadBuffer.Span[0] >> 4);
            if (exponent == 0x0f) throw new Exception("Out of range");

            int mantissa = ((ReadBuffer.Span[0] & 0x0F) >> 4) | (ReadBuffer.Span[1] & 0x0F);

            var luminance = Math.Pow(2, exponent) * mantissa * 0.72;

            return Task.FromResult(new Illuminance(luminance, Illuminance.UnitType.Lux));
        }
    }
}