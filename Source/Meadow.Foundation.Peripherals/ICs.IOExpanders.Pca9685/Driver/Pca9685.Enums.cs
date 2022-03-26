namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9685
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x44
            /// </summary>
            Address_0x44 = 0x40,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x44
        }
    }
}