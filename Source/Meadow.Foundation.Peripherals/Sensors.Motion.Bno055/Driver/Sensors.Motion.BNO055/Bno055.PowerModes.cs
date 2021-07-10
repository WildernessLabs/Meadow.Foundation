using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Bno055
    {
        /// <summary>
        ///     Valis power mode.
        /// </summary>
        public static class PowerModes
        {
            /// <summary>
            ///     All sensors are active in normal mode.
            /// </summary>
            public const byte NORMAL = 0x00;

            /// <summary>
            ///     If no activity is detected for a configuration duration (default = 5s)
            ///     the sensor will enter low power mode.  In this mode, only the accelerometer
            ///     is is active and motion detected by the accelerometer then the device
            ///     will wake.
            /// </summary>
            public const byte LOWPOWER = 0x00;

            /// <summary>
            ///     Put the sensor into Suspoend mode.  In this more no values in
            ///     the register map will be updated.
            /// </summary>
            public const byte SUSPENDED = 0x00;

        }
    }
}
