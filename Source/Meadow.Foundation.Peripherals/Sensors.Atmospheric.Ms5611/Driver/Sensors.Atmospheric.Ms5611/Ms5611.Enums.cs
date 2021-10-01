namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Ms5611
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x5C,
            Default = Address0
        }

        public enum Resolution
        {
            OSR_256 = 0,
            OSR_412 = 1,
            OSR_1024 = 2,
            OSR_2048 = 3,
            OSR_4096 = 4
        }
    }
}
