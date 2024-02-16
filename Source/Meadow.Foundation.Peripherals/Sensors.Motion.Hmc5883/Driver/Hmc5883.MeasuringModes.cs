﻿namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        /// <summary>
        /// HMC5883L measuring mode 
        /// </summary>
        public enum MeasuringModes
        {
            /// <summary>
            /// Continuous measurement
            /// </summary>
            Continuous = 0x00,
            /// <summary>
            /// Single measurement
            /// </summary>
            Single = 0x01
        }
    }
}