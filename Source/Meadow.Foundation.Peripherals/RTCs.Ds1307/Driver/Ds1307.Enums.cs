namespace Meadow.Foundation.RTCs
{
    public partial class Ds1307
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

        public enum SquareWaveFrequency
        {
            Wave_1000Hz,
            Wave_4096Hz,
            Wave_8192Hz,
            Wave_32768Hz,
            Wave_Low,
            Wave_High
        }
    }
}