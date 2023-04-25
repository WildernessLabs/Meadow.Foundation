namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bmp085
    {
		/// <summary>
		/// Valid I2C addresses for the sensor
		/// </summary>
		public enum Addresses : byte
		{
			/// <summary>
			/// Bus address 0x77
			/// </summary>
			Address_0x77 = 0x77,
			/// <summary>
			/// Default bus address
			/// </summary>
			Default = Address_0x77
		}

        /// <summary>
        /// BMP085 device mode
        /// </summary>
        public enum DeviceMode
		{
            /// <summary>
            /// Ultra low power mode
            /// </summary>
            UltraLowPower = 0,
            /// <summary>
            /// Standard / normal mode
            /// </summary>
            Standard = 1,
            /// <summary>
            /// High resolution mode
            /// </summary>
            HighResolution = 2,
            /// <summary>
            /// Ultra high resolution mode
            /// </summary>
            UltraHighResolution = 3
        }
	}
}
