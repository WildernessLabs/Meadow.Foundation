namespace Meadow.Foundation.Sensors.Accelerometers
{
    public partial class Lis2Mdl
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x1E
            /// </summary>
            Address_0x1E = 0x1E,
            /// <summary>
            /// Bus address 0x1C (connect SDO/SA1 pin to the ground)
            /// </summary>
            Address_0x1C = 0x1C,
            /// <summary>
            /// Default I2C address
            /// </summary>
            Default = Address_0x1E,
        }

        /// <summary>
        /// Represents the output data rate (ODR) options for the LIS2MDL magnetometer.
        /// </summary>
        public enum OutputDataRate
        {
            /// <summary>
            /// Output Data Rate: 10 Hz.
            /// </summary>
            Odr10Hz = 0x00,
            /// <summary>
            /// Output Data Rate: 20 Hz.
            /// </summary>
            Odr20Hz = 0x04,
            /// <summary>
            /// Output Data Rate: 50 Hz.
            /// </summary>
            Odr50Hz = 0x08,
            /// <summary>
            /// Output Data Rate: 100 Hz.
            /// </summary>
            Odr100Hz = 0x0C,
        }

        /// <summary>
        /// Represents the operating mode options for the LIS2MDL magnetometer.
        /// </summary>
        public enum OperatingMode
        {
            /// <summary>
            /// Continuous mode: The sensor continuously measures and updates the output registers.
            /// </summary>
            ContinuousMode = 0x00,
            /// <summary>
            /// Single-conversion mode: The sensor performs one measurement and then enters a low-power state until another single-conversion command is received.
            /// </summary>
            SingleConversionMode = 0x01,
            /// <summary>
            /// Power-down mode: The sensor enters a low-power state and stops measuring.
            /// </summary>
            PowerDownMode = 0x03,
        }
    }
}