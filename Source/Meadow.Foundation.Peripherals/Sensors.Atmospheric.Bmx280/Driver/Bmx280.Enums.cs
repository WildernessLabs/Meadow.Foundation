namespace Meadow.Foundation.Sensors.Atmospheric
{
    public abstract partial class Bmx280
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x76
            /// </summary>
            Address_0x76 = 0x76,
            /// <summary>
            /// Bus address 0x77
            /// </summary>
            Address_0x77 = 0x77,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x76
        }

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

        /// <summary>
        /// Valid filter co-efficient values
        /// </summary>
        public enum FilterCoefficient : byte
        {
            /// <summary>
            /// Off
            /// </summary>
            Off = 0,
            /// <summary>
            /// 2x
            /// </summary>
            Two,
            /// <summary>
            /// 4c
            /// </summary>
            Four,
            /// <summary>
            /// 8x
            /// </summary>
            Eight,
            /// <summary>
            /// 16x
            /// </summary>
            Sixteen
        }

        /// <summary>
        /// Valid values for the inactive duration in normal mode.
        /// </summary>
        public enum StandbyDuration : byte
        {
            /// <summary>
            /// 0.5 milliseconds
            /// </summary>
            MsHalf = 0,
            /// <summary>
            /// 62.5 milliseconds
            /// </summary>
            Ms62Half,
            /// <summary>
            /// 125 milliseconds
            /// </summary>
            Ms125,
            /// <summary>
            /// 250 milliseconds
            /// </summary>
            Ms250,
            /// <summary>
            /// 500 milliseconds
            /// </summary>
            Ms500,
            /// <summary>
            /// 1000 milliseconds
            /// </summary>
            Ms1000,
            /// <summary>
            /// 10 milliseconds
            /// </summary>
            Ms10,
            /// <summary>
            /// 20 milliseconds
            /// </summary>
            Ms20
        }

        /// <summary>
        /// Valid values for the operating mode of the sensor.
        /// </summary>
        public enum Modes : byte
        {
            /// <summary>
            /// no operation, all registers accessible, lowest power, selected after startup
            /// </summary>
            Sleep = 0,
            /// <summary>
            /// perform one measurement, store results and return to sleep mode
            /// </summary>
            Forced = 1,
            /// <summary>
            /// perpetual cycling of measurements and inactive periods.
            /// </summary>
            Normal = 3
        }

        /// <summary>
        /// Valid oversampling values.
        /// </summary>
        /// <remarks>
        /// 000 - Data output set to 0x8000
        /// 001 - Oversampling x1
        /// 010 - Oversampling x2
        /// 011 - Oversampling x4
        /// 100 - Oversampling x8
        /// 101, 110, 111 - Oversampling x16
        /// </remarks>
        public enum Oversample : byte
        {
            /// <summary>
            /// No sampling
            /// </summary>
            Skip = 0,
            /// <summary>
            /// 1x oversampling
            /// </summary>
            OversampleX1,
            /// <summary>
            /// 2x oversampling
            /// </summary>
            OversampleX2,
            /// <summary>
            /// 4x oversampling
            /// </summary>
            OversampleX4,
            /// <summary>
            /// 8x oversampling
            /// </summary>
            OversampleX8,
            /// <summary>
            /// 16x oversampling
            /// </summary>
            OversampleX16
        }

        internal enum Register : byte
        {
            ChipID = 0xd0,
            Reset = 0xe0,
            Humidity = 0xf2,
            Status = 0xf3,
            Measurement = 0xf4,
            Configuration = 0xf5,
        }
    }
}