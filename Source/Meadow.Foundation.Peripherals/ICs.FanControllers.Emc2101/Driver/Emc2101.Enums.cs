namespace Meadow.Foundation.ICs.FanControllers
{
    public partial class Emc2101
    {
        /// <summary>
        /// Valid addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x4C
            /// </summary>
            Address_0x4C = 0x4C,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x4C
        }
    }
}