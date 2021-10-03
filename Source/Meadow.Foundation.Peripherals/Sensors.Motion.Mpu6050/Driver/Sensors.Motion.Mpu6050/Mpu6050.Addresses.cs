namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mpu6050
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x68,
            Address1 = 0x69,
            Default = Address0
        }
    }
}
