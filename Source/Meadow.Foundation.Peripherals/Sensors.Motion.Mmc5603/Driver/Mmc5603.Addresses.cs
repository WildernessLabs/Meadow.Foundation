namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mmc5603
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x30 
            /// </summary>
            Address_0x30 = 0x30,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x30
        }
    }
}