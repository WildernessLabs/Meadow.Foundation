using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    ///     Driver for the Max44009 light-to-digital converter.
    /// </summary>
    public class Max44009
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public static byte DefaultI2cAddress = 0x4A; //alt is 0x4B

        private static II2cPeripheral i2cPeripheral;

        public Max44009(II2cBus i2cBus, byte address = 0x4a)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            i2cPeripheral.WriteRegister(0x02, 0x00);
        }

        public double GetIlluminance()
        {
            var data = i2cPeripheral.ReadRegisters(0x03, 2);

            int exponent = ((data[0] & 0xF0) >> 4);
            int mantissa = ((data[0] & 0x0F) >> 4) | (data[1] & 0x0F);

            var luminance = Math.Pow(2, exponent) * mantissa * 0.045;

            return luminance;
        }
    }
}