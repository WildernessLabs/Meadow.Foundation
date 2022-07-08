using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Sgp40
    {
        /// <summary>
        /// Valid addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x59
            /// </summary>
            Address_0x59 = 0x59,
            /// <summary>
            /// Bus address 0x59
            /// </summary>
            Default = Address_0x59
        }

        private static byte[] sgp40_measure_raw_signal = { 0x26, 0x0f };
        private static byte[] sgp40_measure_raw_signal_uncompensated = { 0x26, 0x0F, 0x80, 0x00, 0xA2, 0x66, 0x66, 0x93 };
        private static byte[] sgp40_execute_self_test = { 0x28, 0x0e };
        private static byte[] sgp4x_turn_heater_off = { 0x36, 0x15 };
        private static byte[] sgp4x_get_serial_number = { 0x36, 0x82 };

    }
}
