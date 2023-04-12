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
            AddressMag_0x69 = 0x1E,
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
        /// Enum representing the sensitivity settings for the magnetometer of the LSM303AGR sensor.
        /// </summary>
        public enum MagSensitivity
        {
            /// <summary>
            /// ±50 gauss sensitivity setting for the magnetometer.
            /// </summary>
            Gauss50 = 0x00,
            /// <summary>
            /// ±100 gauss sensitivity setting for the magnetometer.
            /// </summary>
            Gauss100 = 0x20,
            /// <summary>
            /// ±200 gauss sensitivity setting for the magnetometer.
            /// </summary>
            Gauss200 = 0x40,
            /// <summary>
            /// ±400 gauss sensitivity setting for the magnetometer.
            /// </summary>
            Gauss400 = 0x60
        }
    }
}