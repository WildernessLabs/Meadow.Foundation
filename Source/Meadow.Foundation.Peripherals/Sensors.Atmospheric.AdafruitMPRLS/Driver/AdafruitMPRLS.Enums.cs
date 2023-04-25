namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class AdafruitMPRLS
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x18
            /// </summary>
            Address_0x18 = 0x18,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x18
        }
    }
}