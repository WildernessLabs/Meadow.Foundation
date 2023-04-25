namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Si70xx
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
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
        /// Specific device type / model
        /// </summary>
        public enum DeviceType
        {
            /// <summary>
            /// Unknown device
            /// </summary>
            Unknown = 0x00,
            /// <summary>
            /// SI7013
            /// </summary>
            Si7013 = 0x0d,
            /// <summary>
            /// SI7020
            /// </summary>
            Si7020 = 0x14,
            /// <summary>
            /// SI7021
            /// </summary>
            Si7021 = 0x15,
            /// <summary>
            /// Engineering sample
            /// </summary>
            EngineeringSample = 0xff
        }

        /// <summary>
        /// Resolution of sensor data, in bits, for both temperature and humidity
        /// </summary>
        public enum SensorResolution : byte
        {
            /// <summary>
            /// Temperature 14 bits, Humidity 12 bits
            /// </summary>
            TEMP14_HUM12 = 0x00,
            /// <summary>
            /// Temperature 12 bits, Humidity 8 bits
            /// </summary>
            TEMP12_HUM8 = 0x01,
            /// <summary>
            /// Temperature 13 bits, Humidity 10 bits
            /// </summary>
            TEMP13_HUM10 = 0x80,
            /// <summary>
            /// Temperature 11 bits, Humidity 11 bits
            /// </summary>
            TEMP11_HUM11 = 0x81,
        }
    }
}