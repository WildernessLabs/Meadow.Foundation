namespace Meadow.Foundation.Sensors.Camera
{
    public partial class ArducamMini
    {
        /// <summary>
        ///     Valid addresses for the sensor.
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
