using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl345
    {
        /// <summary>
        /// Possible values for the range (see DataFormat register).
        /// </summary>
        /// <remarks>
        /// See page 27 of the data sheet.
        /// </remarks>
        public enum GForceRanges : byte
        {
            TwoG = 0x00,
            FourG = 0x01,
            EightG = 0x02,
            SixteenG = 0x03
        }
    }
}
