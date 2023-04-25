namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an AGS01DB MEMS VOC gas / air quality sensor
    /// Pinout (left to right, label side down): VDD, SDA, GND, SCL
    /// Note: requires pullup resistors on SDA/SCL
    /// </summary>
    public partial class Ags01Db
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x11
            /// </summary>
            Address_0x11 = 0x11,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x11
        }

    }
}
