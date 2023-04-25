namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x39
            /// </summary>
            Address_0x39 = 0x39,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x39
        }
    }
}