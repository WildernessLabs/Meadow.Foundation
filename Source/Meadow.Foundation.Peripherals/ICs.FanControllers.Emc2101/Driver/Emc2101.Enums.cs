namespace Meadow.Foundation.ICs.FanControllers
{
    public partial class Emc2101
    {
        enum Registers : byte
        {
            InternalTemperature = 0x00,
            Status = 0x02,
            Configuration = 0x03,
            DataRate = 0x04,
            FanConfiguration = 0x4A,
            ChipID = 0xFD,
        }

        enum DataRates : byte
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