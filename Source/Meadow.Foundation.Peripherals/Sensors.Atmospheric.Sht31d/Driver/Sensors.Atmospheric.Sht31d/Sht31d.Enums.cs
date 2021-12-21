namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Sht31d
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x44,
            Address1 = 0x45,
            Default = Address0
        }
    }
}
