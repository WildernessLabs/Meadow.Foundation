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
            Ready = 0b_0000_0001,
            Lock = 0b_0000_0010,
            RegulatorEnabled = 0b_0000_0100
        }
    }
}
