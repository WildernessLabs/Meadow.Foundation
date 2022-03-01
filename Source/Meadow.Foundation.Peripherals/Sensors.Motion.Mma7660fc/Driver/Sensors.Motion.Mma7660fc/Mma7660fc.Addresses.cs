namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mma7660fc
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
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