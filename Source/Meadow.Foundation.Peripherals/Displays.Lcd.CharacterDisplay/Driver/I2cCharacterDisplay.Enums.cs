namespace Meadow.Foundation.Displays.Lcd
{
    public partial class I2cCharacterDisplay
    {
        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x27
            /// </summary>
            Address_0x27 = 0x27,
            /// <summary>
            /// Bus address 0x3E
            /// Used by some Grove LCD dislpays
            /// </summary>
            Address_0x3E = 0x3E,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x27,
            /// <summary>
            /// Grove bus address
            /// </summary>
            Grove = Address_0x3E
        }
    }
}