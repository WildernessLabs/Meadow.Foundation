namespace Meadow.Foundation.Audio.Radio
{
    public partial class Tea5767
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// I2C address 0
            /// </summary>
            Address_0x60 = 0x60,
            /// <summary>
            /// Default I2C address
            /// </summary>
            Default = Address_0x60
        }

        enum Command : byte
        {
            FIRST_DATA = 0,
            SECOND_DATA = 1,
            THIRD_DATA = 2,
            FOURTH_DATA = 3,
            FIFTH_DATA = 4,
            LOW_STOP_LEVEL = 1,
            MID_STOP_LEVEL = 2,
            HIGH_STOP_LEVEL = 3,
            HIGH_SIDE_INJECTION = 1,
            LOW_SIDE_INJECTION = 0,
            STEREO_ON = 0,
            STEREO_OFF = 1,
            MUTE_RIGHT_ON = 1,
            MUTE_RIGHT_OFF = 0,
            MUTE_LEFT_ON = 1,
            MUTE_LEFT_OFF = 0,
            SWP1_HIGH = 1,
            SWP1_LOW = 0,
            SWP2_HIGH = 1,
            SWP2_LOW = 0,
            STBY_ON = 1,
            STBY_OFF = 0,
            JAPANESE_FM_BAND = 1,
            US_EUROPE_FM_BAND = 0,
            SOFT_MUTE_ON = 1,
            SOFT_MUTE_OFF = 0,
            HIGH_CUT_CONTROL_ON = 1,
            HIGH_CUT_CONTROL_OFF = 0,
            STEREO_NOISE_CANCELLING_ON = 1,
            STEREO_NOISE_CANCELLING_OFF = 0,
            SEARCH_INDICATOR_ON = 1,
            SEARCH_INDICATOR_OFF = 0,
        }
    }
}