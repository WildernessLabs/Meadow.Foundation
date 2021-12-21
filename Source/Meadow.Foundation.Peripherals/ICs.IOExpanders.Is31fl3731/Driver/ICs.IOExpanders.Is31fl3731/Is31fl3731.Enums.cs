namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Is31fl3731
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x74,
            Address1 = 0x77,
            Default = Address0
        }
    }
}
