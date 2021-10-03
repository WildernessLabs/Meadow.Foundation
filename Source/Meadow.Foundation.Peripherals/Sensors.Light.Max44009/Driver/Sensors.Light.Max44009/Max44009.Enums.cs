namespace Meadow.Foundation.Sensors.Light
{
    public partial class Max44009
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x4A,
            Address1 = 0x4B,
            Default = Address0
        }
    }
}
