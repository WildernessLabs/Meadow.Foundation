namespace Meadow.Foundation.RTCs
{
    public partial class Ds1307
    {
        /// <summary>
		/// Valid addresses for the sensor.
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

        /// <summary>
        /// Square wave frequency
        /// </summary>
        public enum SquareWaveFrequency
        {
            /// <summary>
            /// 1000Hz
            /// </summary>
            Wave_1000Hz,
            /// <summary>
            /// 4096Hz
            /// </summary>
            Wave_4096Hz,
            /// <summary>
            /// 8192Hz
            /// </summary>
            Wave_8192Hz,
            /// <summary>
            /// 32768Hz
            /// </summary>
            Wave_32768Hz,
            /// <summary>
            /// Low
            /// </summary>
            Wave_Low,
            /// <summary>
            /// High
            /// </summary>
            Wave_High
        }
    }
}