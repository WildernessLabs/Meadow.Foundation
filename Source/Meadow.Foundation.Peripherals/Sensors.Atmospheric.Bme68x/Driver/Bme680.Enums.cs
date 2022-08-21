namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        /// <summary>
		/// Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x77
            /// </summary>
            Address_0x77 = 0x77,
            /// <summary>
            /// Bus address 0x76
            /// </summary>
            Address_0x76 = 0x76,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x77
        }

        /// <summary>
        /// 10 addressable heater profiles stored on the Bme680.
        /// </summary>
        public enum HeaterProfile : byte
        {
            /// <summary>
            /// Heating Profile 1
            /// </summary>
            Profile1 = 0b0000,
            /// <summary>
            /// Heating Profile 2
            /// </summary>
            Profile2 = 0b0001,
            /// <summary>
            /// Heating Profile 3
            /// </summary>
            Profile3 = 0b0010,
            /// <summary>
            /// Heating Profile 4
            /// </summary>
            Profile4 = 0b0011,
            /// <summary>
            /// Heating Profile 5
            /// </summary>
            Profile5 = 0b0100,
            /// <summary>
            /// Heating Profile 6
            /// </summary>
            Profile6 = 0b0101,
            /// <summary>
            /// Heating Profile 7
            /// </summary>
            Profile7 = 0b0110,
            /// <summary>
            /// Heating Profile 8
            /// </summary>
            Profile8 = 0b0111,
            /// <summary>
            /// Heating Profile 9
            /// </summary>
            Profile9 = 0b1000,
            /// <summary>
            /// Heating Profile 10
            /// </summary>
            Profile10 = 0b1001
        }

        /// <summary>
        /// BMP680s power modes
        /// </summary>
        public enum PowerMode : byte
        {
            /// <summary>
            /// Power saving mode, no measurements are performed
            /// </summary>
            Sleep = 0b00,
            /// <summary>
            /// Device goes to sleep mode after one measurement
            /// </summary>
            Forced = 0b01
        }
    }
}