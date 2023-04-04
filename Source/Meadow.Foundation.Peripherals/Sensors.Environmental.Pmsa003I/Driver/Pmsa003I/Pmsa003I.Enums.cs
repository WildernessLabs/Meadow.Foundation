namespace Meadow.Foundation.Sensors.Environmental
{
    public partial class Pmsa003I
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        internal enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x52
            /// ADDR is low
            /// </summary>
            Address_0x12 = 0x12,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x12
        }
    }
}