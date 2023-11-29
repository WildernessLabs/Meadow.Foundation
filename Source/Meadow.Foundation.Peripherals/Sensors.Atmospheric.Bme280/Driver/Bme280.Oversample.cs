namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        /// <summary>
        /// Valid oversampling values.
        /// </summary>
        /// <remarks>
        /// 000 - Data output set to 0x8000
        /// 001 - Oversampling x1
        /// 010 - Oversampling x2
        /// 011 - Oversampling x4
        /// 100 - Oversampling x8
        /// 101, 110, 111 - Oversampling x16
        /// </remarks>
        public enum Oversample : byte
        {
            /// <summary>
            /// No sampling
            /// </summary>
            Skip = 0,
            /// <summary>
            /// 1x oversampling
            /// </summary>
            OversampleX1,
            /// <summary>
            /// 2x oversampling
            /// </summary>
            OversampleX2,
            /// <summary>
            /// 4x oversampling
            /// </summary>
            OversampleX4,
            /// <summary>
            /// 8x oversampling
            /// </summary>
            OversampleX8,
            /// <summary>
            /// 16x oversampling
            /// </summary>
            OversampleX16
        }
    }
}
