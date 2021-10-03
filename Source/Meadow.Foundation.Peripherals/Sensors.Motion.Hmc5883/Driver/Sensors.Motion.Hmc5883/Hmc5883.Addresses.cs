namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x1E,
            Address1 = 0x0D,
            Default = Address0,
            Amc5883 = Address1,
        }
    }
}
