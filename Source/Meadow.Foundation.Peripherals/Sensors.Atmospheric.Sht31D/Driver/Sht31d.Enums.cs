namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Sht31d
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x44
            /// </summary>
            Address_0x44 = 0x44,
            /// <summary>
            /// Bus address 0x45
            /// </summary>
            Address_0x45 = 0x45,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x44
        }
    }
}
