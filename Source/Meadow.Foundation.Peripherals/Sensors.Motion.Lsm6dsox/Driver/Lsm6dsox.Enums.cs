namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Lsm6dsox
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x6A (SDO/SA0 low)
            /// </summary>
            Address_0x6A = 0x6A,
            /// <summary>
            /// Bus address 0x6B (SDO/SA0 high)
            /// </summary>
            Address_0x6B = 0x6B,
            /// <summary>
            /// Default I2C address
            /// </summary>
            Default = Address_0x6A,
        }

        /// <summary>
        /// Enum representing the full scale settings for the accelerometer of the LSM6dsox sensor.
        /// </summary>
        public enum AccelFullScale
        {
            /// <summary>
            /// ±2g full scale setting for the accelerometer.
            /// </summary>
            G2 = 0x00,
            /// <summary>
            /// ±4g full scale setting for the accelerometer.
            /// </summary>
            G4 = 0x08,
            /// <summary>
            /// ±8g full scale setting for the accelerometer.
            /// </summary>
            G8 = 0x0C,
            /// <summary>
            /// ±16g full scale setting for the accelerometer.
            /// </summary>
            G16 = 0x04,
        }

        /// <summary>
        /// Represents the available output data rates for the accelerometer and gyroscope.
        /// </summary>
        public enum OutputDataRate
        {
            /// <summary>
            /// Output data rate: 12.5 Hz
            /// </summary>
            Hz12_5 = 0x10,
            /// <summary>
            /// Output data rate: 26 Hz
            /// </summary>
            Hz26 = 0x20,
            /// <summary>
            /// Output data rate: 52 Hz
            /// </summary>
            Hz52 = 0x30,
            /// <summary>
            /// Output data rate: 104 Hz
            /// </summary>
            Hz104 = 0x40,
            /// <summary>
            /// Output data rate: 208 Hz
            /// </summary>
            Hz208 = 0x50,
            /// <summary>
            /// Output data rate: 416 Hz
            /// </summary>
            Hz416 = 0x60,
            /// <summary>
            /// Output data rate: 833 Hz
            /// </summary>
            Hz833 = 0x70,
            /// <summary>
            /// Output data rate: 1660 Hz
            /// </summary>
            Hz1660 = 0x80,
            /// <summary>
            /// Output data rate: 3330 Hz
            /// </summary>
            Hz3330 = 0x90,
            /// <summary>
            /// Output data rate: 6660 Hz
            /// </summary>
            Hz6660 = 0xA0,
        }

        /// <summary>
        /// Enum representing the full scale settings for the gyroscope of the LSM6dsox sensor.
        /// </summary>
        public enum GyroFullScale
        {
            /// <summary>
            /// ±125°/s full scale setting for the gyroscope.
            /// </summary>
            Dps125 = 0x02,
            /// <summary>
            /// ±250°/s full scale setting for the gyroscope.
            /// </summary>
            Dps250 = 0x00,
            /// <summary>
            /// ±500°/s full scale setting for the gyroscope.
            /// </summary>
            Dps500 = 0x04,
            /// <summary>
            /// ±1000°/s full scale setting for the gyroscope.
            /// </summary>
            Dps1000 = 0x08,
            /// <summary>
            /// ±2000°/s full scale setting for the gyroscope.
            /// </summary>
            Dps2000 = 0x0C,
        }
    }
}