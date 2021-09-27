namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class AdafruitMPRLS
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
