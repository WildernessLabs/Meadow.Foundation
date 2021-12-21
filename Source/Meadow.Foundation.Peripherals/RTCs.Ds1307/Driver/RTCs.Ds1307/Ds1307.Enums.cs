namespace Meadow.Foundation.RTCs
{
    public partial class Ds1307
    {
        /// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            Address0 = 0x68,
            Default = Address0
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
