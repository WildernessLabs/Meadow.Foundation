namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Mpl115a2
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
