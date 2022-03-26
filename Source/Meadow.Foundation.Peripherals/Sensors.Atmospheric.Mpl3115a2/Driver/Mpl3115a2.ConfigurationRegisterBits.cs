using System;
namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Mpl3115a2
    {
        /// <summary>
        /// Pressure/Temperature data configuration register bits.
        /// </summary>
        /// <remarks>
        /// For more information see section 7.7 of the datasheet.
        /// </remarks>
        public class ConfigurationRegisterBits
        {
            /// <summary>
            ///     PT_DATA_CFG - Enable the event detection.
            /// </summary>
            public static readonly byte DataReadyEvent = 0x01;

            /// <summary>
            ///     PT_DATA_CFG - Enable the pressure data ready events.
            /// </summary>
            public static readonly byte EnablePressureEvent = 0x02;

            /// <summary>
            ///     PT_DATA_CFG - Enable the temperature data ready events.
            /// </summary>
            public static readonly byte EnableTemperatureEvent = 0x04;
        }
    }
}
