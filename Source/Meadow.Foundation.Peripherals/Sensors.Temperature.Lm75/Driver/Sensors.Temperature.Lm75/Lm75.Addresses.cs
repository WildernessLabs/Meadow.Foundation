namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Lm75
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x48,
            Default = Address0
        }
    }
}
