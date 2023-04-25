namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mma7660fc
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x4C
            /// </summary>
            Address_0x4c = 0x4C,
            /// <summary>
            /// Bus address 0x4C
            /// </summary>
            Default = Address_0x4c
        }
    }
}