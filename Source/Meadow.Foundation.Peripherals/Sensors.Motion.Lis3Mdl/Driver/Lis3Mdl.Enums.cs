namespace Meadow.Foundation.Sensors.Accelerometers
{
    public partial class Lis3Mdl
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x1E (SDO/SA1 high)
            /// </summary>
            Address_0x1D = 0x1E,
            /// <summary>
            /// Bus address 0x1C (SDO/SA1 low)
            /// </summary>
            Address_0x1C = 0x1C,
            /// <summary>
            /// Default I2C address
            /// </summary>
            Default = Address_0x1C,
        }

        /// <summary>
        /// Represents the output data rate (ODR) options for the Lis3MDL magnetometer.
        /// </summary>
        /// <remarks>The bit values here are already shifted to align with the [4:2] bits of the register.</remarks>
        public enum OutputDataRate
        {
            /// <summary>
            /// Output Data Rate: 0.625 Hz.
            /// </summary>
            Odr0Hz625 = 0x00,
            /// <summary>
            /// Output Data Rate: 1.25 Hz.
            /// </summary>
            Odr1Hz25 = 0x04,
            /// <summary>
            /// Output Data Rate: 2.5 Hz.
            /// </summary>
            Odr2Hz5 = 0x08,
            /// <summary>
            /// Output Data Rate: 5 Hz.
            /// </summary>
            Odr5Hz = 0x0C,
            /// <summary>
            /// Output Data Rate: 10 Hz.
            /// </summary>
            Odr10Hz = 0x10,
            /// <summary>
            /// Output Data Rate: 20 Hz.
            /// </summary>
            Odr20Hz = 0x04,
            /// <summary>
            /// Output Data Rate: 40 Hz.
            /// </summary>
            Odr40Hz = 0x18,
            /// <summary>
            /// Output Data Rate: 80 Hz.
            /// </summary>
            Odr80Hz = 0x1C,
        }

        /// <summary>
        /// Represents the output scaling options for the Lis3MDL magnetometer.
        /// </summary>
        /// <remarks>The bit values here are already shifted to align with the [7:6] bits of the register.</remarks>
        public enum FullScale
        {
            /// <summary>
            /// ±4 Gauss range.
            /// </summary>
            PlusMinus4Gauss = 0x00,
            /// <summary>
            /// ±8 Gauss range.
            /// </summary>
            PlusMinus8Gauss = 0x20,
            /// <summary>
            /// ±12 Gauss range.
            /// </summary>
            PlusMinus12Gauss = 0x40,
            /// <summary>
            /// ±16 Gauss range.
            /// </summary>
            PlusMinus16Gauss = 0x60,
        }

        /// <summary>
        /// Represents the operating mode options for the Lis3MDL magnetometer.
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