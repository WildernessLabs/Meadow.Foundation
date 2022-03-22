using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        /// <summary>
        ///     Valid filter co-efficient values.
        /// </summary>
        public enum FilterCoefficient : byte
        {
            Off = 0,
            Two,
            Four,
            Eight,
            Sixteen
        }
    }
}