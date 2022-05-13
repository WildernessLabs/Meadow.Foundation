namespace Meadow.Foundation.Sensors.Camera
{
    public partial class ArducamMini
    {
        // found i2c device at 48 & 172 for ArduCam Mini 2mp plus

        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x21
            /// </summary>
            Address_0x21 = 0x21,
            /// <summary>
            /// Bus address 0x30
            /// </summary>
            Address_0x30 = 0x30,
            /// <summary>
            /// Bus address 0x42
            /// </summary>
            Address_0x42 = 0x42,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x30
        }
    }
}
