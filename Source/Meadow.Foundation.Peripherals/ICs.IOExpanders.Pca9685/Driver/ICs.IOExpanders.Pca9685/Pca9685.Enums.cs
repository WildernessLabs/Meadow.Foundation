namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9685
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x40,
            Default = Address0
        }
    }
}
