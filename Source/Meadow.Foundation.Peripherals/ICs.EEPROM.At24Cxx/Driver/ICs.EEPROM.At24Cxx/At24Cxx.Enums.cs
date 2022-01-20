namespace Meadow.Foundation.ICs.EEPROM
{
    public partial class At24Cxx
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x50
            /// </summary>
            Address_0x50 = 0x50,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x50
        }
    }
}
