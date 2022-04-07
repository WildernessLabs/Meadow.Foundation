namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Htu21d
    {
        enum Registers : byte
        {
            SOFT_RESET = 0xFE,

            TEMPERATURE_MEASURE_NOHOLD = 0xF3,
            HUMDITY_MEASURE_NOHOLD = 0xF5,

            TEMPERATURE_MEASURE_HOLD = 0xE3,
            HUMDITY_MEASURE_HOLD = 0xE5,
            TEMPERATURE_MEASURE_PREVIOUS = 0xE0,

            WRITE_USER_REGISTER = 0xE6,
            READ_USER_REGISTER = 0xE7,
            READ_HEATER_REGISTER = 0x11,
            WRITE_HEATER_REGISTER = 0x51,
        }

        /// <summary>
        /// Resolution of sensor data
        /// </summary>
        public enum SensorResolution : byte
        {
            /// <summary>
            /// 14 bit temperature, 12 bit humidity 
            /// </summary>
            TEMP14_HUM12 = 0x00,
            /// <summary>
            /// 12 bit temperature, 8 bit humidity 
            /// </summary>
            TEMP12_HUM8 = 0x01,
            /// <summary>
            /// 13 bit temperature, 10 bit humidity 
            /// </summary>
            TEMP13_HUM10 = 0x80,
            /// <summary>
            /// 11 bit temperature, 11 bit humidity 
            /// </summary>
            TEMP11_HUM11 = 0x81,
        }
    }
}