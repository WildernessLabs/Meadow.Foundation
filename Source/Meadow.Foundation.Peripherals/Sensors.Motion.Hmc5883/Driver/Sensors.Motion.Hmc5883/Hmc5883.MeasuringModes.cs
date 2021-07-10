using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        /// <summary>
        /// HMC5883L measuring mode 
        /// </summary>
        public enum MeasuringModes
        {
            Continuous = 0x00,
            Single = 0x01
        }
    }
}
