namespace Meadow.Foundation.Sensors.Hid
{
    public partial class Tsc2004
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x4B
            /// </summary>
            Address_0x4B = 0x4B,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x4B
        }
    }
}