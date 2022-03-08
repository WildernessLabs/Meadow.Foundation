using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Htu21d
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x40
            /// </summary>
            Address_0x40 = 0x40,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x40
        }

        private const byte SOFT_RESET = 0xFE;

        private const byte TEMPERATURE_MEASURE_NOHOLD = 0xF3;
        private const byte HUMDITY_MEASURE_NOHOLD = 0xF5;
        /*
        private const byte TEMPERATURE_MEASURE_HOLD = 0xE3;
        private const byte HUMDITY_MEASURE_HOLD = 0xE5;
        private const byte TEMPERATURE_MEASURE_PREVIOUS = 0xE0;
        */
        private const byte WRITE_USER_REGISTER = 0xE6;
        private const byte READ_USER_REGISTER = 0xE7;
        private const byte READ_HEATER_REGISTER = 0x11;
        private const byte WRITE_HEATER_REGISTER = 0x51;

        /// <summary>
        /// Resolution of sensor data
        /// </summary>
        public enum SensorResolution : byte
        {
            TEMP14_HUM12 = 0x00,
            TEMP12_HUM8 = 0x01,
            TEMP13_HUM10 = 0x80,
            TEMP11_HUM11 = 0x81,
        }

        private enum Register : byte
        {

        }
    }
}