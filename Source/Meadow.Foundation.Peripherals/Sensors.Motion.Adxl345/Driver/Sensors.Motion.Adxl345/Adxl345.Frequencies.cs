using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl345
    {
        /// <summary>
        /// Frequency of the sensor readings when the device is in sleep mode.
        /// </summary>
        /// <remarks>
        /// See page 26 of the data sheet.
        /// </remarks>
        public enum Frequencies : byte
        {
            EightHz = 0x00,
            FourHz = 0x01,
            TwoHz = 0x02,
            OneHz = 0x03,
        }
    }
}
