namespace Meadow.Foundation.Sensors.Accelerometers
{
    public partial class Lsm303agr
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x19 for the accelerometer
            /// </summary>
            AddressAccel_0x19 = 0x19,
            /// <summary>
            /// Bus address 0x1E for the magnetometer
            /// </summary>
            AddressMag_0x1E = 0x1E,
        }

        /// <summary>
        /// Enum representing the sensitivity settings for the accelerometer of the LSM303AGR sensor.
        /// </summary>
        public enum AccSensitivity
        {
            /// <summary>
            /// ±2g sensitivity setting for the accelerometer.
            /// </summary>
            G2 = 0x00,
            /// <summary>
            /// ±4g sensitivity setting for the accelerometer.
            /// </summary>
            G4 = 0x10,
            /// <summary>
            /// ±8g sensitivity setting for the accelerometer.
            /// </summary>
            G8 = 0x20,
            /// <summary>
            /// ±16g sensitivity setting for the accelerometer.
            /// </summary>
            G16 = 0x30
        }

        /// <summary>
        /// Represents the available output data rates for the accelerometer.
        /// </summary>
        public enum AccOutputDataRate
        {
            /// <summary>
            /// Output data rate: 1 Hz
            /// </summary>
            Hz1 = 0x10,
            /// <summary>
            /// Output data rate: 10 Hz
            /// </summary>
            Hz10 = 0x20,
            /// <summary>
            /// Output data rate: 25 Hz
            /// </summary>
            Hz25 = 0x30,
            /// <summary>
            /// Output data rate: 50 Hz
            /// </summary>
            Hz50 = 0x40,
            /// <summary>
            /// Output data rate: 100 Hz
            /// </summary>
            Hz100 = 0x50,
            /// <summary>
            /// Output data rate: 200 Hz
            /// </summary>
            Hz200 = 0x60,
            /// <summary>
            /// Output data rate: 400 Hz
            /// </summary>
            Hz400 = 0x70,
            /// <summary>
            /// Output data rate: 1620 Hz
            /// </summary>
            Hz1620 = 0x80,
            /// <summary>
            /// Output data rate: 5376 Hz
            /// </summary>
            Hz5376 = 0x90
        }

        /// <summary>
        /// Represents the available output data rates for the magnetometer.
        /// </summary>
        public enum MagOutputDataRate
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
    }
}