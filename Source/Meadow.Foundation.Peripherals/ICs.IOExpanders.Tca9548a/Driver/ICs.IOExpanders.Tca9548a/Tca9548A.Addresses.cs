namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Tca9548a
    {
        /// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x70
            /// </summary>
            Address_0x70 = 0x70,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x70
        }
    }
}