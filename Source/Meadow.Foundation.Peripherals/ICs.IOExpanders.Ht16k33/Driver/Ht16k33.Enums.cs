namespace Meadow.Foundation.ICs.IOExpanders
{
    //128 LED driver
    //39 key input
    public partial class Ht16k33
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x70
            /// </summary>
            Address_0x70 = 0x70,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x70
        }

        /// <summary>
        /// Blink rate
        /// </summary>
        public enum BlinkRate : byte
        {
            /// <summary>
            /// Off
            /// </summary>
            Off = 0,
            /// <summary>
            /// Fast (2Hz)
            /// </summary>
            Fast = 2, //2hz
            /// <summary>
            /// Medium (1Hz)
            /// </summary>
            Medium = 4, //1hz
            /// <summary>
            /// Slow (0.5Hz)
            /// </summary>
            Slow = 8, //0.5hz
        }

        public enum Brightness : byte
        {
            _0,
            _1,
            _2,
            _3,
            _4,
            _5,
            _6,
            _7,
            _8,
            _9,
            _10,
            _11,
            _12,
            _13,
            _14,
            _15,
            Off = 0,
            Low = 4,
            Medium = 8,
            High = 12,
            Maximum = 15,
        }

        enum Register : byte
        {
            HT16K33_DSP = 128,
            HT16K33_SS = 32, // System setup register
            HT16K33_KDAP = 64, // Key Address Data Pointer
            HT16K33_IFAP = 96, // Read INT flag status
            HT16K33_DIM = 0xE0, // Set brightness / dimmer
            HT16K33_DDAP = 0, //display address pointer
        }
    }
}