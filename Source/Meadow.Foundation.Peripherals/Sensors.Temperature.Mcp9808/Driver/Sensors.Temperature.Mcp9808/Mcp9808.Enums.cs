namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Mcp9808
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x18,
            Default = Address0
        }
    }
}
