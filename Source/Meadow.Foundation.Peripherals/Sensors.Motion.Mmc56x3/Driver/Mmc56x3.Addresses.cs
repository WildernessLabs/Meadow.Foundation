namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mmc56x3
    {
        /// <summary>
        /// Valid addresses for the sensor
        /// </summary>
        public enum Addresses : byte
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