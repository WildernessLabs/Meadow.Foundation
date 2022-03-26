namespace Meadow.Foundation.RTCs
{
    public partial class Ds3231 : Ds323x
    {
        /// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x68
            /// </summary>
            Address_0x68 = 0x68,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x68
        }
    }
}
