namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        /// <summary>
        /// BMx280 type to support both the BME280 and the BMP280
        /// </summary>
        public enum ChipType : byte
        {
            /// <summary>
            /// BMP280
            /// </summary>
            BMP = 0x58,
            /// <summary>
            /// BME280
            /// </summary>
            BME = 0x60
        }
    }
}