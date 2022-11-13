using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        /// <summary>
        /// Sensor measurement time
        /// </summary>
        public enum MeasurementTimeType
        {
            /// <summary>
            /// 160ms
            /// </summary>
            Ms160 = 160,
            /// <summary>
            /// 320ms
            /// </summary>
            Ms320 = 320,
            /// <summary>
            /// 160ms
            /// </summary>
            Ms640 = 640,
            /// <summary>
            /// 1280ms
            /// </summary>
            Ms1280 = 1280,
            /// <summary>
            /// Ms2560ms
            /// </summary>
            Ms2560 = 2560,
            /// <summary>
            /// 5120ms
            /// </summary>
            Ms5120 = 5120
        }
    }
}
