namespace Meadow.Foundation.Sensors.Environmental
{
    partial class Ens160
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x52
            /// ADDR is low
            /// </summary>
            Address_0x52 = 0x52,
            /// <summary>
            /// Bus address 0x53
            /// ADDR is high
            /// </summary>
            Address_0x53 = 0x53,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x52
        }

        /// <summary>
        /// Sensor operating mode
        /// </summary>
        public enum OperatingMode : byte
        {
            /// <summary>
            /// Reset the sensor
            /// </summary>
            Reset = 0xF0,
            /// <summary>
            /// Deep sleep - low power standby mode
            /// </summary>
            DeepSleep = 0x00,
            /// <summary>
            /// Idle - low power mode
            /// </summary>
            Idle = 0x01,
            /// <summary>
            /// Standard gas sensing mode
            /// </summary>
            Standard = 0x02,
        }

        /// <summary>
        /// AQI Rating
        /// </summary>
        /// <remarks>
        /// The AQI-UBA17 air quality index is derived from a guideline by the German Federal Environmental 
        /// Agency based on a TVOC sum signal.Although a local, German guideline, it is referenced and adopted
        /// by many countries and organizations.
        /// </remarks>
        public enum UBAAirQualityIndex
        {
            /// <summary>
            /// Situation not acceptable
            /// Use only if unavoidable 
            /// Intensified ventilation recommended
            /// </summary>
            Unhealthy = 0x05,
            /// <summary>
            /// Major objections
            /// Intensified ventilation recommended
            /// Search for sources
            /// </summary>
            Poor = 0x04,
            /// <summary>
            /// Some objections
            /// Increased ventilation recommended
            /// Search for sources
            /// </summary>
            Moderate = 0x03,
            /// <summary>
            /// No relevant objections
            /// Sufficient ventilation recommended
            /// </summary>
            Good = 0x02,
            /// <summary>
            /// No objections
            /// Target
            /// </summary>
            Excellent = 0x01,
        }

        /// <summary>
        /// Ens160 commands
        /// </summary>
        enum Commands : byte
        {
            NOP = 0x00,
            CLRGPR = 0xCC,
            GET_APPVER = 0x0E,
        }

        /// <summary>
        /// Ens160 commands
        /// </summary>
        enum Registers : byte
        {
            PART_ID         = 0x00,		// 2 byte register
            OPMODE			= 0x10,
            CONFIG			= 0x11,
            COMMAND			= 0x12,
            TEMP_IN			= 0x13,
            RH_IN			= 0x15,
            DATA_STATUS		= 0x20,
            DATA_AQI		= 0x21,
            DATA_TVOC		= 0x22,
            DATA_ETOH       = 0x22,
            DATA_ECO2		= 0x24,			
            DATA_T			= 0x30,
            DATA_RH			= 0x32,
            DATA_MISR		= 0x38,
            GPR_WRITE_0		= 0x40,
            GPR_WRITE_1     = 0x41,
            GPR_WRITE_2		= 0x42,
            GPR_WRITE_3		= 0x43,
            GPR_WRITE_4		= 0x44,
            GPR_WRITE_5		= 0x45,
            GPR_WRITE_6		= 0x46,
            GPR_WRITE_7		= 0x47,
            GPR_READ_0		= 0x48,
            GPR_READ_1      = 0x49,
            GPR_READ_2      = 0x4A,
            GPR_READ_3      = 0x4B,
            GPR_READ_4      = 0x4C,
            GPR_READ_5      = 0x4D,
            GPR_READ_6      = 0x4E,
            GPR_READ_7      = 0x4F,
        }
    }
}