using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        /// <summary>
        ///     Valid values for the operating mode of the sensor.
        /// </summary>
        public enum Modes : byte
        {
            /// <summary>
            /// no operation, all registers accessible, lowest power, selected after startup
            /// </summary>
            Sleep = 0,
            /// <summary>
            /// perform one measurement, store results and return to sleep mode
            /// </summary>
            Forced = 1,
            /// <summary>
            /// perpetual cycling of measurements and inactive periods.
            /// </summary>
            Normal = 3
        }
    }
}
