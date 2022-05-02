namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Sht4x
    {
        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x44
            /// </summary>
            Address_0x44 = 0x44,
            /// <summary>
            /// Bus address 0x45
            /// </summary>
            Address_0x45 = 0x45,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x44
        }

        /// <summary>
        /// Sht4x precision and heater control
        /// </summary>
        public enum Precision : byte
        {
            /// <summary>
            /// High precision no heat
            /// </summary>
            HighPrecisionNoHeat = 0xFD,
            /// <summary>
            /// Medium precision no heat
            /// </summary>
            MediumPrecisionNoHeat = 0xF6,
            /// <summary>
            /// Low precision no heat
            /// </summary>
            LowPrecisionNoHeat = 0xE0,

            /// <summary>
            /// High precision high heat for 1s
            /// </summary>
            HighPrecisionHighHeat1s = 0x39,
            /// <summary>
            /// High precision high heat for 100ms
            /// </summary>
            HighPrecisionHighHeat100ms = 0x32,

            /// <summary>
            /// High precision medium heat for 1s
            /// </summary>
            HighPrecisionMediumHeat1s = 0x2F,
            /// <summary>
            /// High precision medium heat for 100ms
            /// </summary>
            HighPrecisionMediumHeat100ms = 0x24,

            /// <summary>
            /// High precision low heat for 1s
            /// </summary>
            HighPrecisionLowHeat1s = 0x1E,
            /// <summary>
            /// High precision low heat for 100ms
            /// </summary>
            HighPrecisionLowHeat100ms = 0x15,
        }

        enum Commands : int
        {
            MEASURE_HPM = 0xFD,
            MEASURE_LPM = 0xE0,
            READ_SERIAL = 0x89,
            DURATION_USEC = 1000
        }
    }
}
