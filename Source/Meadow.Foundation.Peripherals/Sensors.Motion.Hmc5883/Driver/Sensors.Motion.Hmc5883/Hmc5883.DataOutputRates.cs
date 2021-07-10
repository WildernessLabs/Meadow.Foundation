using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        /// <summary>
        /// HMC5883L Typical Data Output Rate (Hz)
        /// </summary>
        public enum DataOutputRates
        {
            /// <summary>
            /// 0.75 Hz
            /// </summary>
            Rate0_75 = 0x00,

            /// <summary>
            /// 1.5 Hz
            /// </summary>
            Rate1_5 = 0x01,

            /// <summary>
            /// 3 Hz
            /// </summary>
            Rate3 = 0x02,

            /// <summary>
            /// 7.5 Hz
            /// </summary>
            Rate7_5 = 0x03,

            /// <summary>
            /// 15 Hz
            /// </summary>
            Rate15 = 0x04,

            /// <summary>
            /// 30 Hz
            /// </summary>
            Rate30 = 0x05,

            /// <summary>
            /// 75 Hz
            /// </summary>
            Rate75 = 0x06,
        }
    }
}
