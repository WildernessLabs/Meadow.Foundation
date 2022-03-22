namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Address of the peripheral when the address pin is pulled low.
            /// </summary>
            Address_0x38 = 0x38,
            /// <summary>
            /// Address of the peripheral when the address pin is pulled high.
            /// </summary>
            Address_0x39 = 0x39,
            /// <summary>
            /// Defulat bus address
            /// </summary>
            Default = Address_0x38
        }
    }
}
