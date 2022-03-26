namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl345
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x53
            /// </summary>
            Address_0x53 = 0x53,
            /// <summary>
            /// Bus address 0x1D
            /// </summary>
            Address_0x1D = 0x1D,
            /// <summary>
            /// Bus address 0x53
            /// </summary>
            Default = Address_0x53
        }
    }
}
