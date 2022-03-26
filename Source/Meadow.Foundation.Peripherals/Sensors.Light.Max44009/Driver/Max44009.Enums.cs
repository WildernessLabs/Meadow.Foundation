namespace Meadow.Foundation.Sensors.Light
{
    public partial class Max44009
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x4A
            /// </summary>
            Address_0x4A = 0x4A,
            /// <summary>
            /// Bus address 0x4B
            /// </summary>
            Address_0x4B = 0x4B,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x4A
        }
    }
}