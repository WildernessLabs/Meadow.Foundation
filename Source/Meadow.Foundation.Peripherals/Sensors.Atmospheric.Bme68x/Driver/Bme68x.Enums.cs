namespace Meadow.Foundation.Sensors.Atmospheric
{
    partial class Bme68x
    {
        /// <summary>
        /// Valid addresses for the busComms
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
        /// bbME68X heater profiles
        /// </summary>
        public enum HeaterProfileType : byte
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

        /// <summary>
        /// IIR filter coefficient
        /// The higher the coefficient, the slower the sensors respond
        /// </summary>
        public enum FilteringMode
        {
            /// <summary>
            /// Filter coefficient of 0
            /// </summary>
            C0 = 0b000,

            /// <summary>
            /// Filter coefficient of 1
            /// </summary>
            C1 = 0b001,

            /// <summary>
            /// Filter coefficient of 3
            /// </summary>
            C3 = 0b010,

            /// <summary>
            /// Filter coefficient of 7
            /// </summary>
            C7 = 0b011,

            /// <summary>
            /// Filter coefficient of 15
            /// </summary>
            C15 = 0b100,

            /// <summary>
            /// Filter coefficient of 31
            /// </summary>
            C31 = 0b101,

            /// <summary>
            /// Filter coefficient of 63
            /// </summary>
            C63 = 0b110,

            /// <summary>
            /// Filter coefficient of 127
            /// </summary>
            C127 = 0b111
        }
    }
}