namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mag3110
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x0E,
            Address1 = 0x0F,
            Default = Address0
        }
    }
}
