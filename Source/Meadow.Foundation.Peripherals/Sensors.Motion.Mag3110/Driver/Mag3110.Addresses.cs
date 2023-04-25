namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mag3110
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x0E
            /// </summary>
            Address_0x0E = 0x0E,
            /// <summary>
            /// Bus address 0x0F
            /// </summary>
            Address_0x0F = 0x0F,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x0E
        }
    }
}