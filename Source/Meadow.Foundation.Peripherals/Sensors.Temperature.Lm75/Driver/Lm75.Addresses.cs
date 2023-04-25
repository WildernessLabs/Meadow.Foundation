namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Lm75
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x48
            /// </summary>
            Address_0x48 = 0x48,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x48
        }
    }
}
