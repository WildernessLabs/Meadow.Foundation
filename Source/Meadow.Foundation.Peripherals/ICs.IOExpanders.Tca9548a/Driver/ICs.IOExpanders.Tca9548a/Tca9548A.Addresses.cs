namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Tca9548a
    {
        /// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            Address0 = 0x70,
            Default = Address0
        }
    }
}
