using System;

namespace Meadow.Foundation.Sensors.Light
{
    public partial class Veml7700
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x10,
            Default = Address0
        }
    }
}
