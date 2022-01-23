namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x1E
            /// </summary>
            Address_0x1E = 0x1E,
            /// <summary>
            /// Bus address 0x0D
            /// </summary>
            Address_0x0D = 0x0D,
            /// <summary>
            /// Default bus address for Hmc5883
            /// </summary>
            Default = Address_0x1E,
            /// <summary>
            /// Default bus address for Hmc5883
            /// </summary>
            Hmc5883 = Address_0x1E,
            /// <summary>
            /// Default bus address for Amc5883
            /// </summary>
            Qmc5883 = Address_0x0D,
        }
    }
}