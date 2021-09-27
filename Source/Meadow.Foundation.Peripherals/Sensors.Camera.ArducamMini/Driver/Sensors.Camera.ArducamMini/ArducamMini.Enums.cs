namespace Meadow.Foundation.Sensors.Camera
{
    public partial class ArducamMini
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x30,
            Default = Address0
        }
    }
}
