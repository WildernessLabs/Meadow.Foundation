namespace Meadow.Foundation.ICs.FanControllers
{
    public partial class Emc2101
    {
        enum Registers : byte
        {
            InternalTemperature = 0x00,
            ExternalTemperatureLSB = 0x01,
            Status = 0x02,
            Configuration = 0x03,
            DataRate = 0x04,
            ExternalTemperatureMSB = 0x10,
            ForceTemperature = 0x0C, //for LUT testing
            TachLSB = 0x46,
            TachMSB = 0x47,
            TachLimitLSB = 0x48,
            TachLimitMSB = 0x49,
            FanConfiguration = 0x4A,
            FanSpinup = 0x4B,
            FanSetting = 0x4C,
            ChipID = 0xFD,
            PwmFrequency = 0x4D,
            PwmDivisor = 0x4E,
            LutHysteresis = 0x4F,
            LutStartRegister = 0x50,
        }

        /// <summary>
        /// The fan spinup drive
        /// </summary>
        public enum FanSpinupDrive : byte
        {
            /// <summary>
            /// 0 drive
            /// </summary>
            Bypass = 0x00,
            /// <summary>
            /// 50 percent drive
            /// </summary>
            HalfDrive = 0x08,
            /// <summary>
            /// 75 percent drive
            /// </summary>
            ThreeQuartersDrive = 0x09,
            /// <summary>
            /// 100 percent drive
            /// </summary>
            FullDrive = 0x10,
        }

        /// <summary>
        /// The fan spin up time
        /// </summary>
        public enum FanSpinupTime : byte
        {
            /// <summary>
            /// 0 seconds to spin up the fan
            /// </summary>
            _0sec = 0x00,
            /// <summary>
            /// 0.05 seconds to spin up the fan
            /// </summary>
            _0_05sec = 0x01,
            /// <summary>
            /// 0.1 seconds to spin up the fan
            /// </summary>
            _0_1sec = 0x02,
            /// <summary>
            /// 0.2 seconds to spin up the fan
            /// </summary>
            _0_2sec = 0x03,
            /// <summary>
            /// 0.4 seconds to spin up the fan
            /// </summary>
            _0_4sec = 0x04,
            /// <summary>
            /// 0.8 seconds to spin up the fan
            /// </summary>
            _0_8sec = 0x05,
            /// <summary>
            /// 1.6 seconds to spin up the fan
            /// </summary>
            _1_6sec = 0x06,
            /// <summary>
            /// 3.2 seconds to spin up the fan (default)
            /// </summary>
            _3_2sec = 0x07,
        }

        /// <summary>
        /// The EMC2021 data rate
        /// </summary>
        public enum DataRate : byte
        {
            /// <summary>
            /// 1.16hz
            /// </summary>
            _1_16hz,
            /// <summary>
            /// 1.8hz
            /// </summary>
            _1_8hz,
            /// <summary>
            /// 1.4hz
            /// </summary>
            _1_4hz,
            /// <summary>
            /// 1.2hz
            /// </summary>
            _1_2hz,
            /// <summary>
            /// 1hz
            /// </summary>
            _1hz,
            /// <summary>
            /// 2hz
            /// </summary>
            _2hz,
            /// <summary>
            /// 4hz
            /// </summary>
            _4hz,
            /// <summary>
            /// 8hz
            /// </summary>
            _8hz,
            /// <summary>
            /// 16hz
            /// </summary>
            _16hz,
            /// <summary>
            /// 32hz
            /// </summary>
            _32hz,
        }

        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x4C
            /// </summary>
            Address_0x4C = 0x4C,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x4C
        }

        /// <summary>
        /// The temperature lookup table index
        /// </summary>
        public enum LutIndex : byte
        {
            /// <summary>
            /// LUT Index 0
            /// </summary>
            Index0,
            /// <summary>
            /// LUT Index 1
            /// </summary>
            Index1,
            /// <summary>
            /// LUT Index 2
            /// </summary>
            Index2,
            /// <summary>
            /// LUT Index 3
            /// </summary>
            Index3,
            /// <summary>
            /// LUT Index 4
            /// </summary>
            Index4,
            /// <summary>
            /// LUT Index 5
            /// </summary>
            Index5,
            /// <summary>
            /// LUT Index 6
            /// </summary>
            Index6,
            /// <summary>
            /// LUT Index 7
            /// </summary>
            Index7,
        }
    }
}