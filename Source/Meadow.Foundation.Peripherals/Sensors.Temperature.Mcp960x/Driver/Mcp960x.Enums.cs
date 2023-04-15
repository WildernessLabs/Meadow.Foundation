namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Mcp960x
    {
        /// <summary>
        /// Valid addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x60
            /// </summary>
            Address_0x60 = 0x60,
            /// <summary>
            /// Bus address 0x61
            /// </summary>
            Address_0x61 = 0x61,
            /// <summary>
            /// Bus address 0x62
            /// </summary>
            Address_0x62 = 0x62,
            /// <summary>
            /// Bus address 0x63
            /// </summary>
            Address_0x63 = 0x63,
            /// <summary>
            /// Bus address 0x64
            /// </summary>
            Address_0x64 = 0x64,
            /// <summary>
            /// Bus address 0x65
            /// </summary>
            Address_0x65 = 0x65,
            /// <summary>
            /// Bus address 0x60
            /// </summary>
            Address_0x66 = 0x66,
            /// <summary>
            /// Bus address 0x66
            /// </summary>
            Address_0x67 = 0x67,
            /// <summary>
            /// Default bus address 0x67
            /// </summary>
            Default = Address_0x67
        }

        /// <summary>
        /// Represents the supported thermocouple types for the MCP960x
        /// </summary>
        public enum ThermocoupleType
        {
            /// <summary>
            /// K-type thermocouple (chromel–alumel), widely used due to its broad temperature range and durability.
            /// </summary>
            K = 0x00,
            /// <summary>
            /// J-type thermocouple (iron–constantan), commonly used in general-purpose applications.
            /// </summary>
            J = 0x01,
            /// <summary>
            /// T-type thermocouple (copper–constantan), suitable for low-temperature measurements.
            /// </summary>
            T = 0x02,
            /// <summary>
            /// N-type thermocouple (nicrosil–nisil), known for its stability and resistance to high temperatures.
            /// </summary>
            N = 0x03,
            /// <summary>
            /// S-type thermocouple (platinum–rhodium), suitable for high-temperature measurements.
            /// </summary>
            S = 0x04,
            /// <summary>
            /// E-type thermocouple (chromel–constantan), featuring a high output signal and excellent stability.
            /// </summary>
            E = 0x05,
            /// <summary>
            /// B-type thermocouple (platinum–rhodium), used for very high-temperature measurements.
            /// </summary>
            B = 0x06,
            /// <summary>
            /// R-type thermocouple (platinum–rhodium), suitable for high-temperature measurements.
            /// </summary>
            R = 0x07
        }

        /// <summary>
        /// Represents the available ADC resolutions for the MCP960x
        /// </summary>
        public enum AdcResolution
        {
            /// <summary>
            /// 18-bit ADC resolution, providing the fastest conversion time.
            /// </summary>
            _18Bit = 0x00,
            /// <summary>
            /// 16-bit ADC resolution, providing a balance between conversion time and resolution.
            /// </summary>
            _16Bit = 0x01,
            /// <summary>
            /// 14-bit ADC resolution, providing a slower conversion time but higher resolution.
            /// </summary>
            _14Bit = 0x02,
            /// <summary>
            /// 12-bit ADC resolution, providing the slowest conversion time but highest resolution.
            /// </summary>
            _12Bit = 0x03
        }

        /// <summary>
        /// Represents the available filter coefficients for the MCP960x
        /// </summary>
        public enum FilterCoefficient
        {
            /// <summary>
            /// Filter coefficient value 0
            /// </summary>
            _0 = 0x00,
            /// <summary>
            /// Filter coefficient value 1
            /// </summary>
            _1 = 0x01,
            /// <summary>
            /// Filter coefficient value 2
            /// </summary>
            _2 = 0x02,
            /// <summary>
            /// Filter coefficient value 3
            /// </summary>
            _3 = 0x03,
            /// <summary>
            /// Filter coefficient value 4
            /// </summary>
            _4 = 0x04,
            /// <summary>
            /// Filter coefficient value 5
            /// </summary>
            _5 = 0x05,
            /// <summary>
            /// Filter coefficient value 4
            /// </summary>
            _6 = 0x06,
            /// <summary>
            /// Filter coefficient value 5
            /// </summary>
            _7 = 0x07
        }

        /// <summary>
        /// Represents the alert number for the MCP9600.
        /// </summary>
        public enum AlertNumber
        {
            /// <summary>
            /// Alert number 1.
            /// </summary>
            Alert1 = 1,
            /// <summary>
            /// Alert number 2.
            /// </summary>
            Alert2 = 2,
            /// <summary>
            /// Alert number 3.
            /// </summary>
            Alert3 = 3,
            /// <summary>
            /// Alert number 4.
            /// </summary>
            Alert4 = 4
        }
    }
}