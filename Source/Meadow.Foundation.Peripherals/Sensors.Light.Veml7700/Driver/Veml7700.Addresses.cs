namespace Meadow.Foundation.Sensors.Light
{
    public partial class Veml7700
    {
        /// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x10
            /// </summary>
            Address_0x10 = 0x10,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x10
        }
    }
}
