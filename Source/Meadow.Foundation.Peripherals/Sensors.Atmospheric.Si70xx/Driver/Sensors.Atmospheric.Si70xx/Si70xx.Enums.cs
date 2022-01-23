using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Si70xx
    {
        /// <summary>
        /// Valid addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x40
            /// </summary>
            Address_0x40 = 0x40,
            /// <summary>
            /// Bus address 0x40
            /// </summary>
            Default = Address_0x40
        }

        private const byte READ_ID_PART1 = 0xfa;
        private const byte READ_ID_PART2 = 0x0f;
        private const byte READ_2ND_ID_PART1 = 0xfc;
        private const byte READ_2ND_ID_PART2 = 0xc9;

        private const byte TEMPERATURE_MEASURE_NOHOLD = 0xF3;
        private const byte HUMDITY_MEASURE_NOHOLD = 0xF5;

        /*
        private const byte TEMPERATURE_MEASURE_HOLD = 0xE3;
        private const byte HUMDITY_MEASURE_HOLD = 0xE5;
        private const byte TEMPERATURE_MEASURE_PREVIOUS = 0xE0;

        private const byte WRITE_USER_REGISTER = 0xE6;
        private const byte READ_USER_REGISTER = 0xE7;
        private const byte READ_HEATER_REGISTER = 0x11;
        private const byte WRITE_HEATER_REGISTER = 0x51;
        */

        private const byte CMD_RESET = 0xFE;

        private enum Register : byte
        {
            USER_REG_1 = 0x00
        }

        /// <summary>
        ///     Specific device type / model
        /// </summary>
        public enum DeviceType
        {
            Unknown = 0x00,
            Si7013 = 0x0d,
            Si7020 = 0x14,
            Si7021 = 0x15,
            EngineeringSample = 0xff
        }

        /// <summary>
        ///     Resolution of sensor data, in bits, for both temperature and humidity
        /// </summary>
        public enum SensorResolution : byte
        {
            // DEV NOTE: if this is confusing, it's because the resolution bits are D7 and D0 in the register (they are not together)
            TEMP14_HUM12 = 0x00,
            TEMP12_HUM8 = 0x01,
            TEMP13_HUM10 = 0x80,
            TEMP11_HUM11 = 0x81,
        }
    }
}
