using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        /// <summary>
        /// The status of HMC5883L device
        /// </summary>
        public enum Statuses : byte
        {
            /// <summary>
            /// Ready
            /// </summary>
            Ready = 0b_0000_0001,
            /// <summary>
            /// Lock
            /// </summary>
            Lock = 0b_0000_0010,
            /// <summary>
            /// Regulator enabled
            /// </summary>
            RegulatorEnabled = 0b_0000_0100
        }
    }
}
