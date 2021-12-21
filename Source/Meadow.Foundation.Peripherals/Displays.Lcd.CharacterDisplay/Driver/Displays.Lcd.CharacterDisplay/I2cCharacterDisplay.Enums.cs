namespace Meadow.Foundation.Displays.Lcd
{
    public partial class I2cCharacterDisplay
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x27,
            Default = Address0
        }
    }
}
