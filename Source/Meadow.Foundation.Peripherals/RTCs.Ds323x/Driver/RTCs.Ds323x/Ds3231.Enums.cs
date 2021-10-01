namespace Meadow.Foundation.RTCs
{
    public partial class Ds3231 : Ds323x
    {
        /// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            Address0 = 0x68,
            Default = Address0
        }
    }
}
