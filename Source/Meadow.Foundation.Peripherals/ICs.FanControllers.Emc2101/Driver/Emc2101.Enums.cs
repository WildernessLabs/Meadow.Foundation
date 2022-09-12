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
            TachLimitLSB = 0x48,
            TachLimitMSB = 0x49,
            FanConfiguration = 0x4A,
            FanSpinup = 0x4B,
            FanSetting = 0x4C,
            ChipID = 0xFD,
        }

        /// <summary>
        /// The fan spinup drive
        /// </summary>
        public enum SpinDrive : byte
        {
            /// <summary>
            /// 0 drive
            /// </summary>
            Bypass,
            /// <summary>
            /// 50 percent drive
            /// </summary>
            HalfDrive, 
            /// <summary>
            /// 75 percent drive
            /// </summary>
            ThreeQuartersDrive,
            /// <summary>
            /// 100 percent drive
            /// </summary>
            FullDrive
        }

        /// <summary>
        /// The fan spin up time
        /// </summary>
        public enum SpinupTime : byte
        {
            /// <summary>
            /// 0.05 seconds to spin up the fan
            /// </summary>
            _0_05sec,
            /// <summary>
            /// 0.1 seconds to spin up the fan
            /// </summary>
            _0_1sec,
            /// <summary>
            /// 0.2 seconds to spin up the fan
            /// </summary>
            _0_2sec,
            /// <summary>
            /// 0.4 seconds to spin up the fan
            /// </summary>
            _0_4sec,
            /// <summary>
            /// 0.8 seconds to spin up the fan
            /// </summary>
            _0_8sec,
            /// <summary>
            /// 1.6 seconds to spin up the fan
            /// </summary>
            _1_6sec,

        }

        public enum DataRate : byte
        {
            _1_16hz, //1.16hz
            _1_8hz,  //1.8hz
            _1_4hz,  //1.4hz
            _1_2hz,  //1.2hz
            _1hz,    //1hz
            _2hz,    //2hz
            _4hz,    //4hz
            _8hz,    //8hz
            _16hz,   //16hz
            _32hz,   //32hz
        }

        /// <summary>
        /// Valid addresses for the sensor
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
    }
}