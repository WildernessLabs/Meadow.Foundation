namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1750
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Address of the peripheral when the address pin is pulled low
            /// </summary>
            Address_0x5C = 0x5C,
            /// <summary>
            /// Address of the peripheral when the address pin is pulled high
            /// </summary>
            Address_0x23 = 0x23,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x5C
        }
    }
}