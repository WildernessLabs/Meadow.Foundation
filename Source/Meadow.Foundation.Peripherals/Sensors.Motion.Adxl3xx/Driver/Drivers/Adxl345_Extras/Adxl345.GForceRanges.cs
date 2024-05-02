namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl345
    {
        /// <summary>
        /// GForce range
        /// </summary>
        public enum GForceRanges : byte
        {
            /// <summary>
            /// 2x gravity
            /// </summary>
            TwoG = 0x00,
            /// <summary>
            /// 4x gravity
            /// </summary>
            FourG = 0x01,
            /// <summary>
            /// 8x gravity
            /// </summary>
            EightG = 0x02,
            /// <summary>
            /// 16x gravity
            /// </summary>
            SixteenG = 0x03
        }
    }
}