namespace Meadow.Foundation.Sensors.Hid
{
    public abstract partial class WiiExtensionBase
    {
        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x52
            /// </summary>
            Address_0x52 = 0x52,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x52
        }
    }
}