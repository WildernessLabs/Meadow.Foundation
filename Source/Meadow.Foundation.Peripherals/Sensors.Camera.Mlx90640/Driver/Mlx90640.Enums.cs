namespace Meadow.Foundation.Sensors.Camera
{
    public partial class Mlx90640
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x33
            /// </summary>
            Address_0x33 = 0x33,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x33
        }

        /// <summary>
        /// Sensor mode
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Inverted
            /// </summary>
            Interleaved,
            /// <summary>
            /// Chess pattern
            /// </summary>
            Chess
        }

        /// <summary>
        /// Sample resolution
        /// </summary>
        public enum Resolution
        {
            /// <summary>
            /// 16 bits
            /// </summary>
            SixteenBit,
            /// <summary>
            /// 17 bits
            /// </summary>
            SeventeenBit,
            /// <summary>
            /// 17 bits
            /// </summary>
            EighteenBit,
            /// <summary>
            /// 19bits
            /// </summary>
            NineteenBit
        }

        /// <summary>
        /// Camera refresh rate
        /// </summary>
        public enum RefreshRate
        {
            /// <summary>
            /// 0.5hz
            /// </summary>
            _0_5hz,
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
            /// <summary>
            /// 64hz
            /// </summary>
            _64hz
        }
    }
}