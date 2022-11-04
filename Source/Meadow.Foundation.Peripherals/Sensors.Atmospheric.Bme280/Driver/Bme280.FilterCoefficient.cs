using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        /// <summary>
        /// Valid filter co-efficient values
        /// </summary>
        public enum FilterCoefficient : byte
        {
            /// <summary>
            /// Off
            /// </summary>
            Off = 0,
            /// <summary>
            /// 2x
            /// </summary>
            Two,
            /// <summary>
            /// 4c
            /// </summary>
            Four,
            /// <summary>
            /// 8x
            /// </summary>
            Eight,
            /// <summary>
            /// 16x
            /// </summary>
            Sixteen
        }
    }
}