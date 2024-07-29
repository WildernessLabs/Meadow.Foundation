namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Adt7410
    {
        /// <summary>
        /// Enumeration of register addresses for the ADT7410 temperature sensor.
        /// </summary>
        public enum Registers : byte
        {
            /// <summary>
            /// Temperature MSB Register.
            /// Contains the most significant byte of the temperature data.
            /// </summary>
            TEMPMSB = 0x00,

            /// <summary>
            /// Temperature LSB Register.
            /// Contains the least significant byte of the temperature data.
            /// </summary>
            TEMPLSB = 0x01,

            /// <summary>
            /// Status Register.
            /// Provides the current status of the sensor, including flags for various error conditions.
            /// </summary>
            STATUS = 0x02,

            /// <summary>
            /// Configuration Register.
            /// Used for configuring the sensor, such as setting the resolution and operating mode.
            /// </summary>
            CONFIG = 0x03,

            /// <summary>
            /// ID Register.
            /// Contains the identification value of the sensor, typically used to verify the sensor model.
            /// </summary>
            ID = 0x0B,

            /// <summary>
            /// Reset Register.
            /// Writing to this register will reset the sensor to its default state.
            /// </summary>
            RESET = 0x2F
        }
    }
}