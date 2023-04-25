namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Th02
    {
		/// <summary>
		/// Valid I2C addresses for the sensor
		/// </summary>
		public enum Address : byte
		{
			/// <summary>
			/// Bus address 0x40
			/// </summary>
			Address_0x40 = 0x40,
			/// <summary>
			/// Default bus address
			/// </summary>
			Default = Address_0x40
		}

        /// <summary>
        /// Register addresses in the Grove TH02 sensor.
        /// </summary>
        public enum Registers
        {
            /// <summary>
            /// Status register
            /// </summary>
            Status = 0x00,
            /// <summary>
            /// High byte of the data register
            /// </summary>
            DataHigh = 0x01,
            /// <summary>
            /// Low byte of the data register
            /// </summary>
            DataLow = 0x02,
            /// <summary>
            /// Addess of the configuration register
            /// </summary>
            Config = 0x03,
            /// <summary>
            /// Address of the ID register
            /// </summary>
            ID = 0x11
        }
    }
}