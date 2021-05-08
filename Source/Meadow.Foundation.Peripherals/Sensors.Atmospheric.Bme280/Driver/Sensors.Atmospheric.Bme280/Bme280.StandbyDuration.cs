using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        /// <summary>
        ///     Valid values for the inactive duration in normal mode.
        /// </summary>
        public enum StandbyDuration : byte
        {
            /// <summary>
            /// 0.5 milliseconds
            /// </summary>
            MsHalf = 0,
            /// <summary>
            /// 62.5 milliseconds
            /// </summary>
            Ms62Half,
            /// <summary>
            /// 125 milliseconds
            /// </summary>
            Ms125,
            /// <summary>
            /// 250 milliseconds
            /// </summary>
            Ms250,
            /// <summary>
            /// 500 milliseconds
            /// </summary>
            Ms500,
            /// <summary>
            /// 1000 milliseconds
            /// </summary>
            Ms1000,
            /// <summary>
            /// 10 milliseconds
            /// </summary>
            Ms10,
            /// <summary>
            /// 20 milliseconds
            /// </summary>
            Ms20
        }
    }
}
