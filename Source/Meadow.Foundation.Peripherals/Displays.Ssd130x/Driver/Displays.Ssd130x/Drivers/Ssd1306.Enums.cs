namespace Meadow.Foundation.Displays.Ssd130x
{
    public partial class Ssd1306
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x3C,
            Address1 = 0x3D,
            Default = Address0
        }
    }
}