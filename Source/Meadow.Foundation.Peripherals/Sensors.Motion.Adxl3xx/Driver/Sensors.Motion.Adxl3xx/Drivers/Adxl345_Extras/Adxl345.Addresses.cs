namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl345
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x53,
            Address1 = 0x1D,
            Default = Address0
        }
    }
}
