namespace Meadow.Foundation.Sensors.Atmospheric
{
    partial class Bme68x
    {
        /// <summary>
        /// Valid oversampling values
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
            /// No oversampling
            /// </summary>
            Skip = 0,
            /// <summary>
            /// Oversampling x1
            /// </summary>
            OversampleX1,
            /// <summary>
            /// Oversampling xw
            /// </summary>
            OversampleX2,
            /// <summary>
            /// Oversampling x4
            /// </summary>
            OversampleX4,
            /// <summary>
            /// Oversampling x8
            /// </summary>
            OversampleX8,
            /// <summary>
            /// Oversampling x16
            /// </summary>
            OversampleX16
        }
    }
}