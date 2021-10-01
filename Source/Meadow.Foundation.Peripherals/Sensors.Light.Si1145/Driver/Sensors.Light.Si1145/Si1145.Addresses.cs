namespace Meadow.Foundation.Sensors.Light
{
    public partial class Si1145
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x60,
            Default = Address0
        }
    }
}
